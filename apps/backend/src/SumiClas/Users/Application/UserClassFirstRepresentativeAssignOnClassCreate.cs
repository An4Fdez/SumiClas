using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Domain.Events;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

/// <summary>
/// The creator of a class becomes its first delegate (domain doc, section 7.1). Runs in the same transaction as
/// Class creation: if the creator does not exist the handler fails and the class is not saved either.
/// </summary>
public sealed class UserClassFirstRepresentativeAssignOnClassCreate(IUserRepository repository) : IDomainEventHandler<ClassCreatedDomainEvent>
{
    public async Task<Result> Handle(ClassCreatedDomainEvent domainEvent, CancellationToken ct = default)
    {
        var creator = await repository.GetByIdAsync(domainEvent.CreatorId, ct);
        if (creator is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        var representatives = await repository.SearchAsync(
            new UserSpecification(classId: domainEvent.ClassId, classRole: ClassRoleType.ClassRepresentative), ct);
        if (representatives.Count > 0)
        {
            return Result.Failure(UserErrors.ClassRepresentativeAlreadyExists);
        }

        return creator.AssignAsClassRepresentative(domainEvent.ClassId, representatives.Count);
    }
}

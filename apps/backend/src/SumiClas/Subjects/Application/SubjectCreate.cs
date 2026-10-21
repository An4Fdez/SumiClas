using MediatR;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Subjects.Application;

public static class SubjectCreate
{
    public sealed record Command(Guid Id, Guid ClassId, string Label) : IRequest, ICommand;

    public sealed class Create(ISubjectRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var subject = Subject.Create(command.Id, command.ClassId, command.Label);

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);

            var current = userProvider.GetCurrentUserId().OrThrow();

            (await queryBus.Send(new UserClassRepresentativeEnsure.Query(current, command.ClassId), ct)).OrThrow();

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            await repository.AddAsync(subject, ct);
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}

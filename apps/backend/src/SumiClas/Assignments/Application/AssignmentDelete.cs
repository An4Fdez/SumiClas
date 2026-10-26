using MediatR;
using SumiClas.Assignments.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;

namespace SumiClas.Assignments.Application;

public static class AssignmentDelete
{
    /// <summary>The author deletes the homework for good, with its comments and the done marks of the class.</summary>
    public sealed record Command(Guid Id) : IRequest, ICommand;

    public sealed class Delete(IAssignmentRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            Assignment assignment = await repository.GetByIdAsync(command.Id, ct)
                ?? throw new AssignmentExceptions.NotFound();

            if (assignment.AuthorId != current)
            {
                throw new AssignmentExceptions.ChangeForbidden();
            }

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(assignment.ClassId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            repository.Remove(assignment);
        }
    }

    public sealed class Handler(Delete delete) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => delete.ExecuteAsync(command, ct);
    }
}

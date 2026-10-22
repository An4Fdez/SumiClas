using MediatR;
using SumiClas.Assignments.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Assignments.Application;

public static class AssignmentCommentAdd
{
    public sealed record Command(Guid AssignmentId, string Text) : IRequest, ICommand;

    public sealed class Add(IAssignmentRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            Assignment assignment = await repository.GetByIdAsync(command.AssignmentId, ct)
                ?? throw new AssignmentExceptions.NotFound();

            await queryBus.Send(new UserFinderById.Query(current), ct);

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(assignment.ClassId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, assignment.ClassId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            assignment.AddComment(current, command.Text, clock);
        }
    }

    public sealed class Handler(Add add) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => add.ExecuteAsync(command, ct);
    }
}

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
    public sealed record Command(Guid AssignmentId, string Text) : IRequest<Result>, ICommand;

    public sealed class Add(IAssignmentRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();

            var assignment = await repository.GetByIdAsync(command.AssignmentId, ct);
            if (assignment is null)
            {
                return Result.Failure(AssignmentErrors.NotFound);
            }

            await queryBus.Send(new UserFinderById.Query(current), ct);

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(assignment.ClassId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, assignment.ClassId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            return assignment.AddComment(current, command.Text, clock);
        }
    }

    public sealed class Handler(Add add) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => add.ExecuteAsync(command, ct);
    }
}

using MediatR;
using SumiClas.Assignments.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Application;
using SumiClas.Users.Application;

namespace SumiClas.Assignments.Application;

public static class AssignmentCreate
{
    /// <summary>There is no IsProposal here on purpose: it is derived from the author's role in the class.</summary>
    public sealed record Command(
        Guid Id,
        Guid ClassId,
        Guid SubjectId,
        string Title,
        string? Description,
        DateOnly DueDate) : IRequest<Result>, ICommand;

    public sealed class Create(IAssignmentRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var authorId = current.Value;

            // IsProposal depends on a lookup, so the aggregate is built twice: once only to validate the
            // input before any IO (the provisional flag is discarded), and once with the real flag.
            var validation = Assignment.Create(
                command.Id, command.ClassId, command.SubjectId, command.Title, command.Description,
                command.DueDate, authorId, isProposal: true, clock);
            if (validation.IsFailure)
            {
                return Result.Failure(validation.Error!);
            }

            var author = await queryBus.Send(new UserFinderById.Query(authorId), ct);
            if (author.IsFailure)
            {
                return Result.Failure(author.Error!);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result.Failure(schoolClass.Error!);
            }

            var schoolYear = await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.Value.SchoolYearId), ct);
            if (schoolYear.IsFailure)
            {
                return schoolYear;
            }

            var subject = await queryBus.Send(new SubjectFinderById.Query(command.SubjectId), ct);
            if (subject.IsFailure)
            {
                return Result.Failure(subject.Error!);
            }
            if (subject.Value.ClassId != command.ClassId)
            {
                return Result.Failure(AssignmentErrors.SubjectNotInClass);
            }

            var isClassRepresentative = await queryBus.Send(
                new UserIsClassRepresentative.Query(authorId, command.ClassId), ct);

            var creation = Assignment.Create(
                command.Id, command.ClassId, command.SubjectId, command.Title, command.Description,
                command.DueDate, authorId, isProposal: !isClassRepresentative, clock);
            if (creation.IsFailure)
            {
                return Result.Failure(creation.Error!);
            }

            await repository.AddAsync(creation.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}

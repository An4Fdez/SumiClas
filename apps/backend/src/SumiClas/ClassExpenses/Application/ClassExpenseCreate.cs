using MediatR;
using SumiClas.CalendarEvents.Application;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.ClassExpenses.Application;

public static class ClassExpenseCreate
{
    public sealed record Command(
        Guid Id,
        Guid ClassId,
        string Item,
        decimal TotalAmount,
        IReadOnlyCollection<Guid> ParticipantIds,
        Guid? RelatedEventId,
        Guid AuthorId) : IRequest<Result>, ICommand;

    public sealed class UseCase(IClassExpenseRepository classExpenses, IQueryBus queryBus, IClock clock)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var creation = ClassExpense.Create(
                command.Id,
                command.ClassId,
                command.Item,
                command.TotalAmount,
                command.ParticipantIds,
                command.RelatedEventId,
                command.AuthorId,
                clock);
            if (creation.IsFailure)
            {
                return Result.Failure(creation.Error!);
            }

            var author = await queryBus.Send(new UserFinderById.Query(command.AuthorId), ct);
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

            var participantIds = creation.Value.Participants.Select(participant => participant.GuardianId).ToList();
            var participants = await queryBus.Send(new UserFinderByIds.Query(participantIds), ct);
            if (participants.Count != participantIds.Count)
            {
                return Result.Failure(ClassExpenseErrors.ParticipantNotFound);
            }

            if (command.RelatedEventId is { } relatedEventId)
            {
                var relatedEvent = await queryBus.Send(new CalendarEventFinderById.Query(relatedEventId), ct);
                if (relatedEvent.IsFailure)
                {
                    return Result.Failure(relatedEvent.Error!);
                }
            }

            await classExpenses.AddAsync(creation.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}

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
        Guid? RelatedEventId) : IRequest<Result>, ICommand;

    public sealed class Create(IClassExpenseRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();

            var authorId = current;

            var creation = ClassExpense.Create(
                command.Id,
                command.ClassId,
                command.Item,
                command.TotalAmount,
                command.ParticipantIds,
                command.RelatedEventId,
                authorId,
                clock);
            if (creation.IsFailure)
            {
                return Result.Failure(creation.Error!);
            }

            var author = await queryBus.Send(new UserFinderById.Query(authorId), ct);
            if (author.IsFailure)
            {
                return Result.Failure(author.Error!);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);

            var representative = await queryBus.Send(new UserClassRepresentativeEnsure.Query(authorId, command.ClassId), ct);
            if (representative.IsFailure)
            {
                return representative;
            }

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            var participantIds = creation.Value.Participants.Select(participant => participant.GuardianId).ToList();
            var participants = await queryBus.Send(new UserFinderByIds.Query(participantIds), ct);
            if (participants.Count != participantIds.Count)
            {
                return Result.Failure(ClassExpenseErrors.ParticipantNotFound);
            }

            foreach (var participantId in participantIds)
            {
                var membership = await queryBus.Send(new UserClassAccessEnsure.Query(participantId, command.ClassId), ct);
                if (membership.IsFailure)
                {
                    return Result.Failure(ClassExpenseErrors.ParticipantNotInClass);
                }
            }

            if (command.RelatedEventId is { } relatedEventId)
            {
                var relatedEvent = await queryBus.Send(new CalendarEventFinderById.Query(relatedEventId), ct);
                if (relatedEvent.IsFailure)
                {
                    return Result.Failure(relatedEvent.Error!);
                }
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

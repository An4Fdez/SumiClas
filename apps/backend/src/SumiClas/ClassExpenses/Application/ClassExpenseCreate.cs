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
        Guid? RelatedEventId) : IRequest, ICommand;

    public sealed class Create(IClassExpenseRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            Guid authorId = current;

            ClassExpense classExpense = ClassExpense.Create(
                command.Id,
                command.ClassId,
                command.Item,
                command.TotalAmount,
                command.ParticipantIds,
                command.RelatedEventId,
                authorId,
                clock);

            await queryBus.Send(new UserFinderById.Query(authorId), ct);

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);

            await queryBus.Send(new UserClassRepresentativeEnsure.Query(authorId, command.ClassId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            List<Guid> participantIds = classExpense.Participants.Select(participant => participant.GuardianId).ToList();
            IReadOnlyCollection<UserResponse> participants = await queryBus.Send(new UserFinderByIds.Query(participantIds), ct);
            if (participants.Count != participantIds.Count)
            {
                throw new ClassExpenseExceptions.ParticipantNotFound();
            }

            foreach (Guid participantId in participantIds)
            {
                bool isMember = await queryBus.Send(new UserIsClassMember.Query(participantId, command.ClassId), ct);
                if (!isMember)
                {
                    throw new ClassExpenseExceptions.ParticipantNotInClass();
                }
            }

            if (command.RelatedEventId is { } relatedEventId)
            {
                await queryBus.Send(new CalendarEventFinderById.Query(relatedEventId), ct);
            }

            await repository.AddAsync(classExpense, ct);
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}

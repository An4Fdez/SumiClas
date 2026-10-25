using MediatR;
using SumiClas.Announcements.Application;
using SumiClas.CalendarEvents.Application;
using SumiClas.Assignments.Application;
using SumiClas.ClassExpenses.Application;
using SumiClas.LostItems.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.WeeklySummaries.Domain;

namespace SumiClas.WeeklySummaries.Application;

public static class WeeklySummaryGenerate
{
    /// <summary>
    /// Gives the summary of the current week of the class (Monday to Sunday, by the UTC date of the clock), written
    /// only when it is needed: the week is read first, which also checks that the current user belongs to the class;
    /// a week with no news has no text and the model is not asked; a stored summary written from the same data is
    /// returned as it is; otherwise the text is written and stored, replacing an older one of the week. A failure of
    /// the writer (<see cref="WeeklySummaryWriterException"/>) is not caught and nothing is stored.
    /// </summary>
    public sealed record Command(Guid ClassId) : IRequest<WeeklySummaryResponse>, ICommand;

    public sealed class Generate(
        IWeeklySummaryRepository repository,
        IWeeklySummaryWriter writer,
        IQueryBus queryBus,
        IClock clock) : IUseCase
    {
        private const string BirthdayType = "Birthday";

        public async Task<WeeklySummaryResponse> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            DateOnly weekStart = WeeklySummary.WeekStartOf(DateOnly.FromDateTime(clock.UtcNow.UtcDateTime));
            DateOnly weekEnd = WeeklySummary.WeekEndOf(weekStart);

            WeeklySummaryInput input = await ReadWeekAsync(command.ClassId, weekStart, weekEnd, ct);
            if (input.IsEmpty)
            {
                return new WeeklySummaryResponse(weekStart, weekEnd, Content: null, GeneratedAt: null);
            }

            string inputHash = WeeklySummaryFingerprint.Of(WeeklySummaryPromptBuilder.Build(input));
            WeeklySummary? stored = (await repository.SearchAsync(
                    new WeeklySummarySpecification(classId: command.ClassId, weekStart: weekStart), ct))
                .FirstOrDefault();
            if (stored is not null && stored.WasWrittenFrom(inputHash))
            {
                return ResponseOf(stored, weekEnd);
            }

            string content = await writer.WriteAsync(input, ct);
            if (stored is null)
            {
                stored = WeeklySummary.Create(Guid.NewGuid(), command.ClassId, weekStart, content, inputHash, clock);
                await repository.AddAsync(stored, ct);
            }
            else
            {
                stored.Rewrite(content, inputHash, clock);
            }

            return ResponseOf(stored, weekEnd);
        }

        private async Task<WeeklySummaryInput> ReadWeekAsync(
            Guid classId,
            DateOnly from,
            DateOnly to,
            CancellationToken ct)
        {
            IReadOnlyCollection<AnnouncementPeriodResponse> announcements =
                await queryBus.Send(new AnnouncementPeriodFinderByClass.Query(classId, from, to), ct);
            IReadOnlyCollection<CalendarEventPeriodResponse> events =
                await queryBus.Send(new CalendarEventPeriodFinderByClass.Query(classId, from, to), ct);
            IReadOnlyCollection<AssignmentPeriodResponse> assignments =
                await queryBus.Send(new AssignmentPeriodFinderByClass.Query(classId, from, to), ct);
            IReadOnlyCollection<ClassExpenseOpenResponse> expenses =
                await queryBus.Send(new ClassExpenseOpenFinderByClass.Query(classId), ct);
            LostItemOpenResponse lostItems =
                await queryBus.Send(new LostItemOpenFinderByClass.Query(classId), ct);

            return new WeeklySummaryInput(
                from,
                to,
                announcements
                    .Select(announcement => new WeeklySummaryAnnouncement(announcement.Title, announcement.Urgent))
                    .ToList(),
                events
                    .Where(calendarEvent => calendarEvent.Type != BirthdayType)
                    .Select(calendarEvent => new WeeklySummaryEvent(calendarEvent.Title, calendarEvent.Date))
                    .ToList(),
                assignments
                    .Select(assignment => new WeeklySummaryAssignment(assignment.Title, assignment.DueDate))
                    .ToList(),
                expenses.Select(expense => expense.Item).ToList(),
                events.Count(calendarEvent => calendarEvent.Type == BirthdayType),
                lostItems.Count);
        }

        private static WeeklySummaryResponse ResponseOf(WeeklySummary summary, DateOnly weekEnd)
            => new(summary.WeekStart, weekEnd, summary.Content, summary.GeneratedAt);
    }

    public sealed class Handler(Generate generate) : IRequestHandler<Command, WeeklySummaryResponse>
    {
        public Task<WeeklySummaryResponse> Handle(Command command, CancellationToken ct)
            => generate.ExecuteAsync(command, ct);
    }
}

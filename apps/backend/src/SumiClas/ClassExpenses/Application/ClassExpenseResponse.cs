using SumiClas.CalendarEvents.Application;

namespace SumiClas.ClassExpenses.Application;

public sealed record ExpenseParticipantResponse(Guid GuardianId, string FullName, bool HasPaid);

public sealed record ClassExpenseResponse(
    Guid Id,
    Guid ClassId,
    string Item,
    decimal TotalAmount,
    CalendarEventSummaryResponse? RelatedEvent,
    Guid AuthorId,
    string AuthorName,
    string? AuthorRole,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<ExpenseParticipantResponse> Participants,
    DateTimeOffset? UpdatedAt,
    bool IsAuthor);

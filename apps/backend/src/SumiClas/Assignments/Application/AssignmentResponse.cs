namespace SumiClas.Assignments.Application;

public sealed record AssignmentCommentResponse(
    Guid Id,
    Guid AuthorId,
    string AuthorName,
    string? AuthorRole,
    string Text,
    DateTimeOffset CreatedAt);

/// <param name="Done">Whether the current user has marked it as done; the marks of the rest of the class are never exposed.</param>
/// <param name="IsProposal">True when a participant proposed it; false when it is official (published by a delegate).</param>
public sealed record AssignmentResponse(
    Guid Id,
    Guid ClassId,
    Guid SubjectId,
    string SubjectLabel,
    string Title,
    string? Description,
    DateOnly DueDate,
    Guid AuthorId,
    string AuthorName,
    string? AuthorRole,
    bool IsProposal,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<AssignmentCommentResponse> Comments,
    bool Done,
    DateTimeOffset? UpdatedAt,
    bool IsAuthor);

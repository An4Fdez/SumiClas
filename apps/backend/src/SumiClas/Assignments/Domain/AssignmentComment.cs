using SumiClas.Shared.Domain;

namespace SumiClas.Assignments.Domain;

/// <summary>
/// Child entity of the Assignment aggregate ("hilo corto de comentarios" — never
/// meant to grow into free chat). Only Assignment (via AddComment) can create one.
/// </summary>
public sealed class AssignmentComment : Entity
{
    public Guid AuthorId { get; private set; }
    public string Text { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private AssignmentComment(Guid id, Guid authorId, string text, DateTimeOffset createdAt) : base(id)
    {
        AuthorId = authorId;
        Text = text;
        CreatedAt = createdAt;
    }

    internal static Result<AssignmentComment> Create(Guid authorId, string text, IClock clock)
    {
        if (authorId == Guid.Empty)
        {
            return Result<AssignmentComment>.Failure(AssignmentErrors.CommentAuthorRequired);
        }
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result<AssignmentComment>.Failure(AssignmentErrors.CommentTextRequired);
        }

        var comment = new AssignmentComment(Guid.NewGuid(), authorId, text.Trim(), clock.UtcNow);
        return Result<AssignmentComment>.Success(comment);
    }
}

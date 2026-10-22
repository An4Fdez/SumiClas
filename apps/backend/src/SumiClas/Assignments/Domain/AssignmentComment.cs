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

    public AssignmentComment(Guid id, Guid authorId, string text, DateTimeOffset createdAt) : base(id)
    {
        AuthorId = authorId;
        Text = text;
        CreatedAt = createdAt;
    }

    internal static AssignmentComment Create(Guid authorId, string text, IClock clock)
    {
        Validate(authorId, text);

        return new AssignmentComment(Guid.NewGuid(), authorId, text.Trim(), clock.UtcNow);
    }

    private static void Validate(Guid authorId, string text)
    {
        if (authorId == Guid.Empty)
        {
            throw new AssignmentExceptions.CommentAuthorRequired();
        }
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new AssignmentExceptions.CommentTextRequired();
        }
    }
}

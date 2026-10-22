using SumiClas.Shared.Domain;

namespace SumiClas.Assignments.Domain;

public sealed class Assignment : AggregateRoot
{
    private readonly List<AssignmentComment> _comments = new();
    public IReadOnlyCollection<AssignmentComment> Comments => _comments.AsReadOnly();

    /// <summary>Homework always belongs to a specific class, never school-wide.</summary>
    public Guid ClassId { get; private set; }

    public Guid SubjectId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateOnly DueDate { get; private set; }
    public Guid AuthorId { get; private set; }

    /// <summary>true if created by a participant (not an official one from the teacher/class representative), captured at creation time.</summary>
    public bool IsProposal { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Assignment(
        Guid id,
        Guid classId,
        Guid subjectId,
        string title,
        string? description,
        DateOnly dueDate,
        Guid authorId,
        bool isProposal,
        DateTimeOffset createdAt,
        IEnumerable<AssignmentComment> comments) : base(id)
    {
        ClassId = classId;
        SubjectId = subjectId;
        Title = title;
        Description = description;
        DueDate = dueDate;
        AuthorId = authorId;
        IsProposal = isProposal;
        CreatedAt = createdAt;
        _comments.AddRange(comments);
    }

    private Assignment()
    {
        // For reflection purposes (EF Core materialization)
    }

    public static Assignment Create(
        Guid id,
        Guid classId,
        Guid subjectId,
        string title,
        string? description,
        DateOnly dueDate,
        Guid authorId,
        bool isProposal,
        IClock clock)
    {
        Validate(id, classId, subjectId, title, authorId);

        return new Assignment(
            id,
            classId,
            subjectId,
            title.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            dueDate,
            authorId,
            isProposal,
            clock.UtcNow,
            []);
    }

    private static void Validate(Guid id, Guid classId, Guid subjectId, string title, Guid authorId)
    {
        if (id == Guid.Empty)
        {
            throw new AssignmentExceptions.IdRequired();
        }
        if (classId == Guid.Empty)
        {
            throw new AssignmentExceptions.ClassRequired();
        }
        if (subjectId == Guid.Empty)
        {
            throw new AssignmentExceptions.SubjectRequired();
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new AssignmentExceptions.TitleRequired();
        }
        if (authorId == Guid.Empty)
        {
            throw new AssignmentExceptions.AuthorRequired();
        }
    }

    public void AddComment(Guid authorId, string text, IClock clock)
        => _comments.Add(AssignmentComment.Create(authorId, text, clock));
}

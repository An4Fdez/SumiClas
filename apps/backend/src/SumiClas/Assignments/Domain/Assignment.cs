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

    private Assignment(
        Guid id,
        Guid classId,
        Guid subjectId,
        string title,
        string? description,
        DateOnly dueDate,
        Guid authorId,
        bool isProposal,
        DateTimeOffset createdAt) : base(id)
    {
        ClassId = classId;
        SubjectId = subjectId;
        Title = title;
        Description = description;
        DueDate = dueDate;
        AuthorId = authorId;
        IsProposal = isProposal;
        CreatedAt = createdAt;
    }

    public static Result<Assignment> Create(
        Guid classId,
        Guid subjectId,
        string title,
        string? description,
        DateOnly dueDate,
        Guid authorId,
        bool isProposal,
        IClock clock)
    {
        var validation = Validate(classId, subjectId, title, authorId);
        if (validation.IsFailure)
        {
            return Result<Assignment>.Failure(validation.Error!);
        }

        var assignment = new Assignment(
            Guid.NewGuid(),
            classId,
            subjectId,
            title.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            dueDate,
            authorId,
            isProposal,
            clock.UtcNow);

        return Result<Assignment>.Success(assignment);
    }

    private static Result Validate(Guid classId, Guid subjectId, string title, Guid authorId)
    {
        if (classId == Guid.Empty)
        {
            return Result.Failure(AssignmentErrors.ClassRequired);
        }
        if (subjectId == Guid.Empty)
        {
            return Result.Failure(AssignmentErrors.SubjectRequired);
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(AssignmentErrors.TitleRequired);
        }
        if (authorId == Guid.Empty)
        {
            return Result.Failure(AssignmentErrors.AuthorRequired);
        }
        return Result.Success();
    }

    public Result AddComment(Guid authorId, string text, IClock clock)
    {
        var comment = AssignmentComment.Create(authorId, text, clock);
        if (comment.IsFailure)
        {
            return Result.Failure(comment.Error!);
        }

        _comments.Add(comment.Value);
        return Result.Success();
    }
}

using SumiClas.Shared.Domain;

namespace SumiClas.Assignments.Domain;

public sealed class AssignmentSpecification : Specification<Assignment>
{
    /// <param name="commentedByUserId">Matches assignments that have a comment from this user.</param>
    /// <param name="dueFrom">Matches assignments due on this day or later.</param>
    /// <param name="dueTo">Matches assignments due on this day or earlier.</param>
    /// <param name="notDoneByUserId">Matches assignments this user has not marked as done.</param>
    public AssignmentSpecification(
        Guid? id = null,
        Guid? classId = null,
        Guid? subjectId = null,
        string? title = null,
        string? description = null,
        DateOnly? dueDate = null,
        Guid? authorId = null,
        bool? isProposal = null,
        DateTimeOffset? createdAt = null,
        Guid? commentedByUserId = null,
        DateOnly? dueFrom = null,
        DateOnly? dueTo = null,
        Guid? notDoneByUserId = null,
        KeysetPage<DateOnly>? page = null)
    {
        if (id is { } assignmentId) AddCriteria(assignment => assignment.Id == assignmentId);
        if (classId is { } schoolClass) AddCriteria(assignment => assignment.ClassId == schoolClass);
        if (subjectId is { } subject) AddCriteria(assignment => assignment.SubjectId == subject);
        if (title is not null) AddCriteria(assignment => assignment.Title == title);
        if (description is not null) AddCriteria(assignment => assignment.Description == description);
        if (dueDate is { } due) AddCriteria(assignment => assignment.DueDate == due);
        if (authorId is { } author) AddCriteria(assignment => assignment.AuthorId == author);
        if (isProposal is { } proposal) AddCriteria(assignment => assignment.IsProposal == proposal);
        if (createdAt is { } created) AddCriteria(assignment => assignment.CreatedAt == created);
        if (commentedByUserId is { } commenter) AddCriteria(assignment => assignment.Comments.Any(comment => comment.AuthorId == commenter));
        if (dueFrom is { } from) AddCriteria(assignment => assignment.DueDate >= from);
        if (dueTo is { } to) AddCriteria(assignment => assignment.DueDate <= to);
        if (notDoneByUserId is { } user) AddCriteria(assignment => assignment.Completions.All(completion => completion.UserId != user));
        if (page is not null) AddKeysetPage(assignment => assignment.DueDate, assignment => assignment.Id, page, descending: false);
    }
}

namespace SumiClas.Tests.Assignments.Domain;

using Moq;
using SumiClas.Assignments.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public static class AssignmentMother
{
    private const int DAYS_AHEAD_OF_SOME_DAY = 7;

    public static readonly DateOnly SomeDay =
        DateOnly.FromDateTime(UnitTestCase.Now.UtcDateTime).AddDays(DAYS_AHEAD_OF_SOME_DAY);

    public static Assignment With(
        Guid? id = null,
        Guid? classId = null,
        Guid? subjectId = null,
        DateOnly? dueDate = null,
        Guid? authorId = null,
        bool isProposal = false,
        string? description = null,
        IReadOnlyCollection<AssignmentComment>? comments = null,
        string? title = null)
        => new(
            id ?? GuidMother.Random(),
            classId ?? GuidMother.Random(),
            subjectId ?? GuidMother.Random(),
            title ?? StringMother.Random("Homework"),
            description,
            dueDate ?? SomeDay,
            authorId ?? GuidMother.Random(),
            isProposal,
            UnitTestCase.Now,
            comments ?? []);

    public static Assignment DoneBy(
        Guid assignmentId,
        Guid userId,
        Guid? classId = null,
        DateOnly? dueDate = null,
        string? title = null)
    {
        Mock<IClock> clock = new();
        clock.Setup(source => source.UtcNow).Returns(UnitTestCase.Now);
        Assignment assignment = With(id: assignmentId, classId: classId, dueDate: dueDate, title: title);
        assignment.MarkAsDoneBy(userId, clock.Object);
        return assignment;
    }
}

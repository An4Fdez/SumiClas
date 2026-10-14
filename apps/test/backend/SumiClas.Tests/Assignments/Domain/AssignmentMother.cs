namespace SumiClas.Tests.Assignments.Domain;

using SumiClas.Assignments.Domain;
using SumiClas.Tests.Shared;

public static class AssignmentMother
{
    public static readonly DateOnly SomeDay = DateOnly.FromDateTime(UnitTestCase.Now.UtcDateTime).AddDays(7);

    public static Assignment With(
        Guid? id = null,
        Guid? classId = null,
        Guid? subjectId = null,
        DateOnly? dueDate = null,
        Guid? authorId = null,
        bool isProposal = false,
        string? description = null,
        IReadOnlyCollection<AssignmentComment>? comments = null)
        => new(
            id ?? GuidMother.Random(),
            classId ?? GuidMother.Random(),
            subjectId ?? GuidMother.Random(),
            StringMother.Random("Homework"),
            description,
            dueDate ?? SomeDay,
            authorId ?? GuidMother.Random(),
            isProposal,
            UnitTestCase.Now,
            comments ?? []);
}

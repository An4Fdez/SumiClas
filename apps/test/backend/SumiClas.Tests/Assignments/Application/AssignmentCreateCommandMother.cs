namespace SumiClas.Tests.Assignments.Application;

using SumiClas.Assignments.Application;
using SumiClas.Tests.Assignments.Domain;
using SumiClas.Tests.Shared;

public static class AssignmentCreateCommandMother
{
    public static AssignmentCreate.Command With(
        Guid id,
        Guid classId,
        Guid subjectId,
        string title,
        string? description,
        DateOnly dueDate)
        => new(id, classId, subjectId, title, description, dueDate);

    public static AssignmentCreate.Command Valid(
        Guid? id = null,
        Guid? classId = null,
        Guid? subjectId = null,
        string? title = null,
        string? description = null,
        DateOnly? dueDate = null)
        => With(
            id ?? GuidMother.Random(),
            classId ?? GuidMother.Random(),
            subjectId ?? GuidMother.Random(),
            title ?? StringMother.Random("Homework"),
            description,
            dueDate ?? AssignmentMother.SomeDay);
}

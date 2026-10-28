namespace SumiClas.Tests.Assignments.Application;

using SumiClas.Assignments.Application;
using SumiClas.Tests.Assignments.Domain;
using SumiClas.Tests.Shared;

public static class AssignmentUpdateCommandMother
{
    public static AssignmentUpdate.Command With(
        Guid id,
        Guid subjectId,
        string title,
        string? description,
        DateOnly dueDate)
        => new(id, subjectId, title, description, dueDate);

    public static AssignmentUpdate.Command Valid(Guid id, string? title = null)
        => With(id, GuidMother.Random(), title ?? StringMother.Random("Homework"), null, AssignmentMother.SomeDay);
}

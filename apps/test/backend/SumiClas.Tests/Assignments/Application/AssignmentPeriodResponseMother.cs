namespace SumiClas.Tests.Assignments.Application;

using SumiClas.Assignments.Application;
using SumiClas.Tests.Shared;

public static class AssignmentPeriodResponseMother
{
    public static AssignmentPeriodResponse With(DateOnly dueDate, string? title = null)
        => new(title ?? StringMother.Random("assignment"), dueDate);
}

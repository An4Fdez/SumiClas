namespace SumiClas.Tests.Assignments.Application;

using SumiClas.Assignments.Application;

public static class AssignmentPeriodFinderByClassQueryMother
{
    public static AssignmentPeriodFinderByClass.Query With(Guid classId, DateOnly from, DateOnly to)
        => new(classId, from, to);
}

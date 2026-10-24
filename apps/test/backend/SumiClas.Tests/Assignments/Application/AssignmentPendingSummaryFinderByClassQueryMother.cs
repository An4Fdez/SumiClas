namespace SumiClas.Tests.Assignments.Application;

using SumiClas.Assignments.Application;

public static class AssignmentPendingSummaryFinderByClassQueryMother
{
    public static AssignmentPendingSummaryFinderByClass.Query WithClassId(Guid classId) => new(classId);
}

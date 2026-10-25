namespace SumiClas.Tests.Assignments.Application;

using SumiClas.Assignments.Application;

public static class AssignmentPendingSummaryResponseMother
{
    public static AssignmentPendingSummaryResponse None() => new(Pending: 0, NextDueDate: null);
}

namespace SumiClas.Tests.Assignments.Application;

using SumiClas.Assignments.Application;

public static class AssignmentDoneUnmarkCommandMother
{
    public static AssignmentDoneUnmark.Command WithAssignmentId(Guid assignmentId) => new(assignmentId);
}

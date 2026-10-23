namespace SumiClas.Tests.Assignments.Application;

using SumiClas.Assignments.Application;

public static class AssignmentDoneMarkCommandMother
{
    public static AssignmentDoneMark.Command WithAssignmentId(Guid assignmentId) => new(assignmentId);
}

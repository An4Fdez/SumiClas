namespace SumiClas.Tests.Assignments.Application;

using SumiClas.Assignments.Application;
using SumiClas.Tests.Shared;

public static class AssignmentCommentAddCommandMother
{
    public static AssignmentCommentAdd.Command WithAssignmentId(Guid assignmentId)
        => new(assignmentId, StringMother.Random("Comment"));

    public static AssignmentCommentAdd.Command WithText(Guid assignmentId, string text) => new(assignmentId, text);
}

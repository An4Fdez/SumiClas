namespace SumiClas.Tests.Assignments.Domain;

using SumiClas.Assignments.Domain;
using SumiClas.Tests.Shared;

public static class AssignmentCommentMother
{
    public static AssignmentComment With(Guid authorId, string text, DateTimeOffset createdAt)
        => new(GuidMother.Random(), authorId, text, createdAt);
}

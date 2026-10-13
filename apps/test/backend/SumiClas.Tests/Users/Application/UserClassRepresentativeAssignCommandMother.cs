namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserClassRepresentativeAssignCommandMother
{
    public static UserClassRepresentativeAssign.Command With(Guid userId, Guid classId) => new(userId, classId);
}

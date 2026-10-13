namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserClassRepresentativeResignCommandMother
{
    public static UserClassRepresentativeResign.Command WithClassId(Guid classId) => new(classId);
}

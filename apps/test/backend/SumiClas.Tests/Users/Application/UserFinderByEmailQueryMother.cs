namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserFinderByEmailQueryMother
{
    public static UserFinderByEmail.Query WithEmail(string email) => new(email);
}

namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserPasswordVerifyQueryMother
{
    public static UserPasswordVerify.Query Create(string email, string password) => new(email, password);

    public static UserPasswordVerify.Query WithCredentials(string email, string password) => Create(email, password);

    public static UserPasswordVerify.Query WithNullFields() => Create(null!, null!);
}

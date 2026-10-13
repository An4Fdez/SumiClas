namespace SumiClas.Tests.Users.Domain;

using SumiClas.Users.Domain;

public static class PasswordMother
{
    public static Password Create(string value) => Password.Create(value).Value;
}

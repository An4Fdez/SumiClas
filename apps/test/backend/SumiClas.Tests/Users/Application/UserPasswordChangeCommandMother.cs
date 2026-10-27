namespace SumiClas.Tests.Users.Application;

using SumiClas.Tests.Shared;
using SumiClas.Users.Application;

public static class UserPasswordChangeCommandMother
{
    public static UserPasswordChange.Command With(string? currentPassword = null, string? newPassword = null)
        => new(currentPassword ?? StringMother.Random("current"), newPassword ?? StringMother.Random("new-password"));
}

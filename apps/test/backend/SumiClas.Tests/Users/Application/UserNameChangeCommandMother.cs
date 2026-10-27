namespace SumiClas.Tests.Users.Application;

using SumiClas.Tests.Shared;
using SumiClas.Users.Application;

public static class UserNameChangeCommandMother
{
    public static UserNameChange.Command With(string? fullName = null)
        => new(fullName ?? StringMother.Random("User"));
}

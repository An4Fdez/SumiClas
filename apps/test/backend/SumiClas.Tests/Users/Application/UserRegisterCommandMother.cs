namespace SumiClas.Tests.Users.Application;

using SumiClas.Tests.Shared;
using SumiClas.Users.Application;

public static class UserRegisterCommandMother
{
    public static UserRegister.Command Random() => With();

    public static UserRegister.Command With(
        Guid? id = null,
        string? fullName = null,
        string? email = null,
        string? password = null)
        => new(
            id ?? GuidMother.Random(),
            fullName ?? StringMother.Random("User"),
            email ?? GuidMother.Random().ToString("N") + "@example.com",
            password ?? StringMother.Random("password"));

    public static UserRegister.Command WithId(Guid id) => With(id: id);

    public static UserRegister.Command WithPassword(string password) => With(password: password);

    public static UserRegister.Command WithEmail(string email) => With(email: email);

    public static UserRegister.Command WithFullName(string fullName) => With(fullName: fullName);
}

namespace SumiClas.Tests.Users.Domain;

using SumiClas.Users.Application;

public static class UserResponseMother
{
    public static UserResponse Create(Guid id, string fullName)
        => new(id, fullName);

    public static UserResponse With(Guid id, string fullName) => Create(id, fullName);
}

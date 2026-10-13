namespace SumiClas.Tests.Sessions.Application;

using SumiClas.Sessions.Application;

public static class SessionRotateCommandMother
{
    public static SessionRotate.Command Create(string? refreshToken)
        => new(refreshToken);

    public static SessionRotate.Command WithToken(string? refreshToken) => Create(refreshToken);
}

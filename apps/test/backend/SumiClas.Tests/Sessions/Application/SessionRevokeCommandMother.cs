namespace SumiClas.Tests.Sessions.Application;

using SumiClas.Sessions.Application;

public static class SessionRevokeCommandMother
{
    public static SessionRevoke.Command Create(string? refreshToken)
        => new(refreshToken);

    public static SessionRevoke.Command WithToken(string? refreshToken) => Create(refreshToken);
}

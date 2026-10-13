namespace SumiClas.Tests.Sessions.Application;

using SumiClas.Sessions.Application;

public static class SessionResponseMother
{
    public static SessionResponse Create(
        string accessToken,
        DateTimeOffset expiresAt,
        Guid userId,
        string fullName,
        string refreshTokenRaw,
        DateTimeOffset refreshTokenExpiresAt)
        => new(accessToken, expiresAt, userId, fullName, refreshTokenRaw, refreshTokenExpiresAt);
}

namespace SumiClas.Sessions.Application;

/// <summary>
/// What a login or a refresh hands to the API layer. <see cref="RefreshTokenRaw"/> belongs in the cookie only and must
/// never be serialized into a response body.
/// </summary>
public sealed record SessionResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    string FullName,
    string RefreshTokenRaw);
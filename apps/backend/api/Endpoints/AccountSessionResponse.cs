namespace SumiClas.Api.Endpoints;

/// <summary>What a login or a refresh tells the client. The refresh token travels only in its cookie, never in this body.</summary>
public sealed record AccountSessionResponse(string AccessToken, DateTimeOffset ExpiresAt, Guid UserId, string FullName);

namespace SumiClas.Api.Endpoints;

/// <summary>What a successful login tells the client. Tokens never travel in this body.</summary>
public sealed record AccountResponse(Guid UserId, string FullName);

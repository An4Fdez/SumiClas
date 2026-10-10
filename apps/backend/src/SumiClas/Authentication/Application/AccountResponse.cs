namespace SumiClas.Authentication.Application;

/// <summary>What a successful login tells the client. No token yet: that comes with the JWT plan.</summary>
public sealed record AccountResponse(Guid UserId, string FullName);

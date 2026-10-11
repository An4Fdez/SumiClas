namespace SumiClas.Authentication.Application;

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

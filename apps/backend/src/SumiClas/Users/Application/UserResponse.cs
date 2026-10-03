namespace SumiClas.Users.Application;

/// <summary>Deliberately minimal (no email): other modules only need to know who the user is.</summary>
public sealed record UserResponse(Guid Id, string FullName);

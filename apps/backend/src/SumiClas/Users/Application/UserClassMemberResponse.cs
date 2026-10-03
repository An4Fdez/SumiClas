namespace SumiClas.Users.Application;

/// <summary>A user as shown inside a class: name plus the role they hold there (null when they have none).</summary>
public sealed record UserClassMemberResponse(Guid UserId, string FullName, string? ClassRole);

namespace SumiClas.Users.Application;

public sealed record UserClassRoleResponse(Guid ClassId, string Role);

/// <summary>The profile of the signed-in user themself (so it includes their own email, unlike the minimal <see cref="UserResponse"/>).</summary>
public sealed record UserProfileResponse(Guid Id, string FullName, string Email, IReadOnlyCollection<UserClassRoleResponse> ClassRoles);

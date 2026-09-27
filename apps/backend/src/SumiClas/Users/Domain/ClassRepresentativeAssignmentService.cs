using SumiClas.Shared.Domain;

namespace SumiClas.Users.Domain;

/// <summary>
/// Stateless Domain Service for the invariant "at most N class representatives per class",
/// which depends on counting OTHER User aggregates and therefore cannot be protected by a
/// single User instance. The future Application layer resolves currentClassRepresentativeCount
/// via IUserRepository.CountClassRepresentativesAsync before calling this service.
/// </summary>
public sealed class ClassRepresentativeAssignmentService
{
    public const int MaxClassRepresentatives = 3;

    public Result AssignClassRepresentative(User user, Guid classId, int currentClassRepresentativeCount)
    {
        var alreadyRepresentative = user.ClassRoles
            .Any(r => r.ClassId == classId && r.Role == ClassRoleType.ClassRepresentative);

        if (!alreadyRepresentative && currentClassRepresentativeCount >= MaxClassRepresentatives)
        {
            return Result.Failure(UserErrors.MaxClassRepresentativesReached);
        }

        return user.AssignAsClassRepresentative(classId);
    }
}

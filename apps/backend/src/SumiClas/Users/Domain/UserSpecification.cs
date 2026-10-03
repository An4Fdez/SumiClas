using SumiClas.Shared.Domain;

namespace SumiClas.Users.Domain;

public sealed class UserSpecification : Specification<User>
{
    /// <param name="classId">Matches users that have a role in this class.</param>
    /// <param name="classRole">Matches users that hold this role (in <paramref name="classId"/> when it is also given).</param>
    public UserSpecification(
        Guid? id = null,
        string? fullName = null,
        Email? email = null,
        Guid? classId = null,
        ClassRoleType? classRole = null,
        IReadOnlyCollection<Guid>? ids = null)
    {
        if (id is { } userId) AddCriteria(user => user.Id == userId);
        if (fullName is not null) AddCriteria(user => user.FullName == fullName);
        if (email is not null) AddCriteria(user => user.Email.Value == email.Value);
        if (ids is not null)
        {
            var userIds = ids.ToArray();
            AddCriteria(user => userIds.Contains(user.Id));
        }

        if (classId is not null || classRole is not null)
        {
            AddCriteria(user => user.ClassRoles.Any(role =>
                (classId == null || role.ClassId == classId) && (classRole == null || role.Role == classRole)));
        }
    }
}

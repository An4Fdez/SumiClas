using SumiClas.Shared.Domain;

namespace SumiClas.Users.Domain;

public sealed class UserSpecification : Specification<User>
{
    /// <param name="classId">Matches users that have a role in this class.</param>
    /// <param name="classRole">Matches users that hold this role (in <paramref name="classId"/> or <paramref name="classIds"/> when also given).</param>
    /// <param name="classIds">Matches users that have a role in any of these classes; an empty list matches nobody.</param>
    public UserSpecification(
        Guid? id = null,
        string? fullName = null,
        Email? email = null,
        Guid? classId = null,
        ClassRoleType? classRole = null,
        IReadOnlyCollection<Guid>? ids = null,
        IReadOnlyCollection<Guid>? classIds = null)
    {
        if (id is { } userId) AddCriteria(user => user.Id == userId);
        if (fullName is not null) AddCriteria(user => user.FullName == fullName);
        if (email is not null) AddCriteria(user => user.Email.Value == email.Value);
        if (ids is not null)
        {
            Guid[] userIds = ids.ToArray();
            AddCriteria(user => userIds.Contains(user.Id));
        }

        Guid[]? classIdList = classIds?.ToArray();
        if (classId is not null || classRole is not null || classIdList is not null)
        {
            AddCriteria(user => user.ClassRoles.Any(role =>
                (classId == null || role.ClassId == classId)
                && (classIdList == null || classIdList.Contains(role.ClassId))
                && (classRole == null || role.Role == classRole)));
        }
    }
}

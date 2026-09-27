using SumiClas.Shared.Domain;

namespace SumiClas.Users.Domain;

public sealed class ClassRoleAssignment : ValueObject
{
    public Guid ClassId { get; }
    public ClassRoleType Role { get; }

    public ClassRoleAssignment(Guid classId, ClassRoleType role)
    {
        ClassId = classId;
        Role = role;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ClassId;
        yield return Role;
    }
}

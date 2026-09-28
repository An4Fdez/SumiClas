using SumiClas.Shared.Domain;

namespace SumiClas.Users.Domain;

public sealed class User : AggregateRoot
{
    private readonly List<ClassRoleAssignment> _classRoles = new();
    public IReadOnlyCollection<ClassRoleAssignment> ClassRoles => _classRoles.AsReadOnly();

    public string FullName { get; private set; }
    public Email Email { get; private set; }

    private User(Guid id, string fullName, Email email) : base(id)
    {
        FullName = fullName;
        Email = email;
    }

    /// <summary>EF Core materialization only — constructor parameters can never bind to an owned-type navigation (Email), even when named to match.</summary>
    private User()
    {
        FullName = null!;
        Email = null!;
    }

    public static Result<User> Register(string fullName, Email email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result<User>.Failure(UserErrors.FullNameRequired);
        }

        return Result<User>.Success(new User(Guid.NewGuid(), fullName.Trim(), email));
    }

    /// <summary>Default role when joining a class — always allowed, no limit.</summary>
    public Result AssignAsParticipant(Guid classId)
    {
        return Upsert(classId, ClassRoleType.Participant);
    }

    /// <summary>
    /// Protects only the LOCAL invariant. Does NOT validate the global cap of 3
    /// class representatives (it can't: this instance has no visibility into other
    /// User aggregates) — see ClassRepresentativeAssignmentService for that check.
    /// </summary>
    public Result AssignAsClassRepresentative(Guid classId)
    {
        return Upsert(classId, ClassRoleType.ClassRepresentative);
    }

    public Result ResignAsClassRepresentative(Guid classId)
    {
        var current = _classRoles.FirstOrDefault(r => r.ClassId == classId);
        if (current is null)
        {
            return Result.Failure(UserErrors.NoRoleAssignedInClass);
        }
        if (current.Role != ClassRoleType.ClassRepresentative)
        {
            return Result.Failure(UserErrors.NotClassRepresentative);
        }

        return Upsert(classId, ClassRoleType.Participant);
    }

    private Result Upsert(Guid classId, ClassRoleType role)
    {
        if (classId == Guid.Empty)
        {
            return Result.Failure(UserErrors.ClassIdRequired);
        }

        _classRoles.RemoveAll(r => r.ClassId == classId);
        _classRoles.Add(new ClassRoleAssignment(classId, role));
        return Result.Success();
    }
}

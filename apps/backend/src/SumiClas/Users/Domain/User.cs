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

    public static Result<User> Register(Guid id, string fullName, Email email)
    {
        var validation = Validate(id, fullName);
        if (validation.IsFailure)
        {
            return Result<User>.Failure(validation.Error!);
        }

        return Result<User>.Success(new User(id, fullName.Trim(), email));
    }

    private static Result Validate(Guid id, string fullName)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure(UserErrors.IdRequired);
        }
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result.Failure(UserErrors.FullNameRequired);
        }
        return Result.Success();
    }

    /// <summary>Default role when joining a class — always allowed, no limit.</summary>
    public Result AssignAsParticipant(Guid classId)
    {
        return Upsert(classId, ClassRoleType.Participant);
    }

    public const int MaxClassRepresentativesPerClass = 3;

    /// <summary>
    /// A class has at most <see cref="MaxClassRepresentativesPerClass"/> representatives. This instance cannot see the
    /// other User aggregates, so the caller (a use case or event handler with the repository) supplies how many
    /// representatives the class has now. Assigning the role to someone who already holds it is always allowed.
    /// </summary>
    public Result AssignAsClassRepresentative(Guid classId, int currentClassRepresentativeCount)
    {
        var alreadyRepresentative = _classRoles
            .Any(r => r.ClassId == classId && r.Role == ClassRoleType.ClassRepresentative);

        if (!alreadyRepresentative && currentClassRepresentativeCount >= MaxClassRepresentativesPerClass)
        {
            return Result.Failure(UserErrors.MaxClassRepresentativesReached);
        }

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

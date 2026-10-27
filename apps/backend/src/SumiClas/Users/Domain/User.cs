using SumiClas.Shared.Domain;
using SumiClas.Shared.Domain.Events;

namespace SumiClas.Users.Domain;

public sealed class User : AggregateRoot
{
    private readonly List<ClassRoleAssignment> _classRoles = new();
    public IReadOnlyCollection<ClassRoleAssignment> ClassRoles => _classRoles.AsReadOnly();

    public string FullName { get; private set; }
    public Email Email { get; private set; }

    /// <summary>Only the hash is kept, never the password.</summary>
    public string PasswordHash { get; private set; }
    public DateTimeOffset PasswordChangedAt { get; private set; }

    public User(
        Guid id,
        string fullName,
        Email email,
        string passwordHash,
        DateTimeOffset passwordChangedAt,
        IEnumerable<ClassRoleAssignment> classRoles) : base(id)
    {
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        PasswordChangedAt = passwordChangedAt;
        _classRoles.AddRange(classRoles);
    }

    private User()
    {
        // For reflection purposes (EF Core materialization)
    }

    public static User Register(Guid id, string fullName, Email email, string passwordHash, IClock clock)
    {
        Validate(id, fullName, passwordHash);

        return new User(id, fullName.Trim(), email, passwordHash, clock.UtcNow, []);
    }

    private static void Validate(Guid id, string fullName, string passwordHash)
    {
        if (id == Guid.Empty)
        {
            throw new UserExceptions.IdRequired();
        }
        ValidateFullName(fullName);
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new UserExceptions.PasswordHashRequired();
        }
    }

    private static void ValidateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new UserExceptions.FullNameRequired();
        }
    }

    public void Rename(string fullName)
    {
        ValidateFullName(fullName);

        FullName = fullName.Trim();
    }

    /// <summary>
    /// Replaces the password for another one the user chose. Unlike <see cref="ReplacePasswordHash"/>, which stores a
    /// stronger hash of the same password, this announces the change so every session of the user can be closed.
    /// </summary>
    public void ChangePassword(string newHash, IClock clock)
    {
        ReplacePasswordHash(newHash, clock);

        AddDomainEvent(new UserPasswordChangedDomainEvent(Id, clock));
    }

    /// <summary>Stores a newer hash of the same password (the hasher asks for it when its parameters have been strengthened).</summary>
    public void ReplacePasswordHash(string newHash, IClock clock)
    {
        if (string.IsNullOrWhiteSpace(newHash))
        {
            throw new UserExceptions.PasswordHashRequired();
        }

        PasswordHash = newHash;
        PasswordChangedAt = clock.UtcNow;
    }

    /// <summary>Default role when joining a class — always allowed, no limit.</summary>
    public void AssignAsParticipant(Guid classId)
    {
        Upsert(classId, ClassRoleType.Participant);
    }

    public const int MaxClassRepresentativesPerClass = 3;

    /// <summary>
    /// A class has at most <see cref="MaxClassRepresentativesPerClass"/> representatives. This instance cannot see the
    /// other User aggregates, so the caller (a use case or event handler with the repository) supplies how many
    /// representatives the class has now. Assigning the role to someone who already holds it is always allowed.
    /// </summary>
    public void AssignAsClassRepresentative(Guid classId, int currentClassRepresentativeCount)
    {
        bool alreadyRepresentative = _classRoles
            .Any(r => r.ClassId == classId && r.Role == ClassRoleType.ClassRepresentative);

        if (!alreadyRepresentative && currentClassRepresentativeCount >= MaxClassRepresentativesPerClass)
        {
            throw new UserExceptions.MaxClassRepresentativesReached();
        }

        Upsert(classId, ClassRoleType.ClassRepresentative);
    }

    public void ResignAsClassRepresentative(Guid classId)
    {
        ClassRoleAssignment current = _classRoles.FirstOrDefault(r => r.ClassId == classId)
            ?? throw new UserExceptions.NoRoleAssignedInClass();
        if (current.Role != ClassRoleType.ClassRepresentative)
        {
            throw new UserExceptions.NotClassRepresentative();
        }

        Upsert(classId, ClassRoleType.Participant);
    }

    private void Upsert(Guid classId, ClassRoleType role)
    {
        if (classId == Guid.Empty)
        {
            throw new UserExceptions.ClassIdRequired();
        }

        _classRoles.RemoveAll(r => r.ClassId == classId);
        _classRoles.Add(new ClassRoleAssignment(classId, role));
    }
}

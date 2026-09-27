using SumiClas.Shared.Domain;

namespace SumiClas.Students.Domain;

public sealed class Student : AggregateRoot
{
    private readonly List<GuardianLink> _guardians = new();
    public IReadOnlyCollection<GuardianLink> Guardians => _guardians.AsReadOnly();

    public StudentName Name { get; private set; }

    private Student(Guid id, StudentName name, GuardianLink firstGuardian) : base(id)
    {
        Name = name;
        _guardians.Add(firstGuardian);
    }

    /// <summary>EF Core materialization only — the real constructor takes firstGuardian, which doesn't map to a single property.</summary>
    private Student()
    {
        Name = null!;
    }

    /// <summary>
    /// A Student is always registered by a guardian (section 7.2 of the domain doc):
    /// it never exists without at least one guardian, not even transiently.
    /// </summary>
    public static Result<Student> Register(StudentName name, Guid firstGuardianId, GuardianRole firstGuardianRole)
    {
        if (firstGuardianId == Guid.Empty)
        {
            return Result<Student>.Failure(StudentErrors.FirstGuardianRequired);
        }

        var guardianLink = GuardianLink.Create(firstGuardianId, firstGuardianRole);
        if (guardianLink.IsFailure)
        {
            return Result<Student>.Failure(guardianLink.Error!);
        }

        var student = new Student(Guid.NewGuid(), name, guardianLink.Value);
        return Result<Student>.Success(student);
    }

    public Result AddGuardian(Guid guardianId, GuardianRole role)
    {
        if (_guardians.Any(g => g.GuardianId == guardianId))
        {
            return Result.Failure(StudentErrors.GuardianAlreadyLinked);
        }

        var guardianLink = GuardianLink.Create(guardianId, role);
        if (guardianLink.IsFailure)
        {
            return Result.Failure(guardianLink.Error!);
        }

        _guardians.Add(guardianLink.Value);
        return Result.Success();
    }

    public Result RemoveGuardian(Guid guardianId)
    {
        var link = _guardians.FirstOrDefault(g => g.GuardianId == guardianId);
        if (link is null)
        {
            return Result.Failure(StudentErrors.GuardianNotLinked);
        }

        if (_guardians.Count == 1)
        {
            return Result.Failure(StudentErrors.AtLeastOneGuardianRequired);
        }

        _guardians.Remove(link);
        return Result.Success();
    }
}

using SumiClas.Shared.Domain;

namespace SumiClas.Students.Domain;

public sealed class Student : AggregateRoot
{
    private readonly List<GuardianLink> _guardians = new();
    public IReadOnlyCollection<GuardianLink> Guardians => _guardians.AsReadOnly();

    public StudentName Name { get; private set; }

    public Student(Guid id, StudentName name, IEnumerable<GuardianLink> guardians) : base(id)
    {
        Name = name;
        _guardians.AddRange(guardians);
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
    public static Result<Student> Register(Guid id, StudentName name, Guid firstGuardianId, GuardianRole firstGuardianRole)
    {
        var validation = Validate(id, firstGuardianId);
        if (validation.IsFailure)
        {
            return Result<Student>.Failure(validation.Error!);
        }

        var guardianLink = GuardianLink.Create(firstGuardianId, firstGuardianRole);
        if (guardianLink.IsFailure)
        {
            return Result<Student>.Failure(guardianLink.Error!);
        }

        var student = new Student(id, name, [guardianLink.Value]);
        return Result<Student>.Success(student);
    }

    private static Result Validate(Guid id, Guid firstGuardianId)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure(StudentErrors.IdRequired);
        }
        if (firstGuardianId == Guid.Empty)
        {
            return Result.Failure(StudentErrors.FirstGuardianRequired);
        }
        return Result.Success();
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

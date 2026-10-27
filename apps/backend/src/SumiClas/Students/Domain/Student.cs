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

    private Student()
    {
        // For reflection purposes (EF Core materialization)
    }

    /// <summary>
    /// A Student is always registered by a guardian (section 7.2 of the domain doc):
    /// it never exists without at least one guardian, not even transiently.
    /// </summary>
    public static Student Register(Guid id, StudentName name, Guid firstGuardianId, GuardianRole firstGuardianRole)
    {
        Validate(id, firstGuardianId);

        GuardianLink guardianLink = GuardianLink.Create(firstGuardianId, firstGuardianRole);

        return new Student(id, name, [guardianLink]);
    }

    private static void Validate(Guid id, Guid firstGuardianId)
    {
        if (id == Guid.Empty)
        {
            throw new StudentExceptions.IdRequired();
        }
        if (firstGuardianId == Guid.Empty)
        {
            throw new StudentExceptions.FirstGuardianRequired();
        }
    }

    public void Rename(StudentName name)
    {
        Name = name;
    }

    public void AddGuardian(Guid guardianId, GuardianRole role)
    {
        if (_guardians.Any(g => g.GuardianId == guardianId))
        {
            throw new StudentExceptions.GuardianAlreadyLinked();
        }

        _guardians.Add(GuardianLink.Create(guardianId, role));
    }

    public void RemoveGuardian(Guid guardianId)
    {
        GuardianLink link = _guardians.FirstOrDefault(g => g.GuardianId == guardianId)
            ?? throw new StudentExceptions.GuardianNotLinked();

        if (_guardians.Count == 1)
        {
            throw new StudentExceptions.AtLeastOneGuardianRequired();
        }

        _guardians.Remove(link);
    }
}

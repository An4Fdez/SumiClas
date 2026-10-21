using SumiClas.Shared.Domain;

namespace SumiClas.Students.Domain;

public sealed class GuardianLink : ValueObject
{
    public Guid GuardianId { get; }
    public GuardianRole Role { get; }

    private GuardianLink(Guid guardianId, GuardianRole role)
    {
        GuardianId = guardianId;
        Role = role;
    }

    public static GuardianLink Create(Guid guardianId, GuardianRole role)
    {
        if (guardianId == Guid.Empty)
        {
            throw new StudentExceptions.GuardianIdRequired();
        }

        return new GuardianLink(guardianId, role);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return GuardianId;
        yield return Role;
    }
}

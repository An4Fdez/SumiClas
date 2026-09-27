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

    public static Result<GuardianLink> Create(Guid guardianId, GuardianRole role)
    {
        if (guardianId == Guid.Empty)
        {
            return Result<GuardianLink>.Failure(StudentErrors.GuardianIdRequired);
        }

        return Result<GuardianLink>.Success(new GuardianLink(guardianId, role));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return GuardianId;
        yield return Role;
    }
}

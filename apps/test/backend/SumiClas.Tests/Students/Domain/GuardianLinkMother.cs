namespace SumiClas.Tests.Students.Domain;

using SumiClas.Students.Domain;

public static class GuardianLinkMother
{
    public static GuardianLink Create(Guid guardianId, GuardianRole role)
        => GuardianLink.Create(guardianId, role).Value;

    public static GuardianLink Mother(Guid guardianId) => Create(guardianId, GuardianRole.Mother);

    public static GuardianLink Father(Guid guardianId) => Create(guardianId, GuardianRole.Father);
}

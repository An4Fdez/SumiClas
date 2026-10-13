namespace SumiClas.Tests.Students.Domain;

using SumiClas.Students.Domain;
using SumiClas.Tests.Shared;

public static class StudentMother
{
    public static Student GuardedBy(Guid motherId, params Guid[] otherGuardianIds)
        => new(
            GuidMother.Random(),
            StudentNameMother.Random(),
            [GuardianLinkMother.Mother(motherId), .. otherGuardianIds.Select(GuardianLinkMother.Father)]);
}

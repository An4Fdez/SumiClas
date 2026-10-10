using SumiClas.Schools.Domain;
using SumiClas.Shared.Domain;

namespace SumiClas.Tests.Schools.Domain;

[TestFixture]
public class SchoolCreateTests
{
    private static readonly Guid SchoolId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
    private static readonly Guid LocalityId = Guid.Parse("7a1d5e90-2b3c-4d6e-8f10-a1b2c3d4e5f6");
    private static readonly Guid PostalCodeId = Guid.Parse("c0ffee00-1111-4222-8333-444455556666");

    [Test]
    public void Create_WithValidData_TrimsAndNormalizesTheName()
    {
        var result = School.Create(SchoolId, "  CEIP Peña Blanca ", "  28000001 ", LocalityId, PostalCodeId, "Calle Mayor 1");

        Assert.That(result.IsSuccess, Is.True);
        var school = result.Value;
        Assert.That(school.Name, Is.EqualTo("CEIP Peña Blanca"));
        Assert.That(school.NormalizedName, Is.EqualTo("ceip pena blanca"));
        Assert.That(school.OfficialCode, Is.EqualTo("28000001"));
        Assert.That(school.LocalityId, Is.EqualTo(LocalityId));
        Assert.That(school.PostalCodeId, Is.EqualTo(PostalCodeId));
        Assert.That(school.Address, Is.EqualTo("Calle Mayor 1"));
    }

    [Test]
    public void Create_WithoutPostalCodeOrAddress_LeavesThemNull()
    {
        var result = School.Create(SchoolId, "CEIP Sumi", "28000001", LocalityId);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.PostalCodeId, Is.Null);
        Assert.That(result.Value.Address, Is.Null);
    }

    [Test]
    public void Create_WithPhone_TrimsIt()
    {
        var result = School.Create(SchoolId, "CEIP Sumi", "28000001", LocalityId, phone: "  952 000 000 ");

        Assert.That(result.Value.Phone, Is.EqualTo("952 000 000"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithoutUsablePhone_LeavesItNull(string? phone)
    {
        var result = School.Create(SchoolId, "CEIP Sumi", "28000001", LocalityId, phone: phone);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Phone, Is.Null);
    }

    [Test]
    public void Create_WithEmptyId_ReturnsIdRequired()
        => AssertFailure(School.Create(Guid.Empty, "CEIP Sumi", "28000001", LocalityId), SchoolErrors.IdRequired);

    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithBlankName_ReturnsNameRequired(string name)
        => AssertFailure(School.Create(SchoolId, name, "28000001", LocalityId), SchoolErrors.NameRequired);

    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithBlankOfficialCode_ReturnsOfficialCodeRequired(string officialCode)
        => AssertFailure(School.Create(SchoolId, "CEIP Sumi", officialCode, LocalityId), SchoolErrors.OfficialCodeRequired);

    [Test]
    public void Create_WithoutLocality_ReturnsLocalityRequired()
        => AssertFailure(School.Create(SchoolId, "CEIP Sumi", "28000001", Guid.Empty), SchoolErrors.LocalityRequired);

    [Test]
    public void Create_WithEmptyPostalCodeId_ReturnsPostalCodeInvalid()
        => AssertFailure(School.Create(SchoolId, "CEIP Sumi", "28000001", LocalityId, Guid.Empty), SchoolErrors.PostalCodeInvalid);

    private static void AssertFailure(Result<School> result, Error expected)
    {
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(expected));
    }
}

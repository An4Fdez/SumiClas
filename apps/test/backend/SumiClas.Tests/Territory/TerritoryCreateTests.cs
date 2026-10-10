using SumiClas.Localities.Domain;
using SumiClas.PostalCodes.Domain;
using SumiClas.Provinces.Domain;
using SumiClas.Shared.Domain;

namespace SumiClas.Tests.Territory;

[TestFixture]
public class TerritoryCreateTests
{
    private static readonly Guid Id = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
    private static readonly Guid OtherId = Guid.Parse("7a1d5e90-2b3c-4d6e-8f10-a1b2c3d4e5f6");

    [Test]
    public void Province_WithValidData_KeepsNameAndNormalizedName()
    {
        var result = Province.Create(Id, "  Málaga ");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo("Málaga"));
        Assert.That(result.Value.NormalizedName, Is.EqualTo("malaga"));
    }

    [Test]
    public void Province_WithEmptyId_ReturnsIdRequired()
        => AssertFailure(Province.Create(Guid.Empty, "Málaga"), ProvinceErrors.IdRequired);

    [Test]
    public void Province_WithBlankName_ReturnsNameRequired()
        => AssertFailure(Province.Create(Id, "  "), ProvinceErrors.NameRequired);

    [Test]
    public void Locality_WithValidData_KeepsProvinceAndNormalizedName()
    {
        var result = Locality.Create(Id, OtherId, " Fuengirola ");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ProvinceId, Is.EqualTo(OtherId));
        Assert.That(result.Value.Name, Is.EqualTo("Fuengirola"));
        Assert.That(result.Value.NormalizedName, Is.EqualTo("fuengirola"));
    }

    [Test]
    public void Locality_WithEmptyId_ReturnsIdRequired()
        => AssertFailure(Locality.Create(Guid.Empty, OtherId, "Fuengirola"), LocalityErrors.IdRequired);

    [Test]
    public void Locality_WithoutProvince_ReturnsProvinceRequired()
        => AssertFailure(Locality.Create(Id, Guid.Empty, "Fuengirola"), LocalityErrors.ProvinceRequired);

    [Test]
    public void Locality_WithBlankName_ReturnsNameRequired()
        => AssertFailure(Locality.Create(Id, OtherId, ""), LocalityErrors.NameRequired);

    [Test]
    public void PostalCode_WithFiveDigits_IsCreated()
    {
        var result = PostalCode.Create(Id, OtherId, " 29640 ");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.LocalityId, Is.EqualTo(OtherId));
        Assert.That(result.Value.Code, Is.EqualTo("29640"));
    }

    [Test]
    public void PostalCode_WithEmptyId_ReturnsIdRequired()
        => AssertFailure(PostalCode.Create(Guid.Empty, OtherId, "29640"), PostalCodeErrors.IdRequired);

    [Test]
    public void PostalCode_WithoutLocality_ReturnsLocalityRequired()
        => AssertFailure(PostalCode.Create(Id, Guid.Empty, "29640"), PostalCodeErrors.LocalityRequired);

    [TestCase("")]
    [TestCase("2964")]
    [TestCase("296400")]
    [TestCase("2964a")]
    public void PostalCode_WithoutExactlyFiveDigits_ReturnsCodeInvalid(string code)
        => AssertFailure(PostalCode.Create(Id, OtherId, code), PostalCodeErrors.CodeInvalid);

    private static void AssertFailure<T>(Result<T> result, Error expected)
    {
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(expected));
    }
}

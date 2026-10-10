using Moq;
using SumiClas.Localities.Application;
using SumiClas.Localities.Domain;
using SumiClas.PostalCodes.Application;
using SumiClas.PostalCodes.Domain;
using SumiClas.Provinces.Application;
using SumiClas.Provinces.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Tests.Territory;

[TestFixture]
public class TerritoryFindersTests
{
    private static readonly Guid Almeria = Guid.Parse("a0000000-0000-4000-8000-000000000001");
    private static readonly Guid Madrid = Guid.Parse("a0000000-0000-4000-8000-000000000002");

    private List<Province> _provinces = null!;
    private List<Locality> _localities = null!;
    private List<PostalCode> _postalCodes = null!;
    private IProvinceRepository _provinceRepository = null!;
    private ILocalityRepository _localityRepository = null!;
    private IPostalCodeRepository _postalCodeRepository = null!;

    [SetUp]
    public void SetUp()
    {
        _provinces = [];
        _localities = [];
        _postalCodes = [];

        var provinces = new Mock<IProvinceRepository>();
        provinces
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<Province>>(), It.IsAny<CancellationToken>()))
            .Returns<ISpecification<Province>, CancellationToken>((specification, _) =>
                Task.FromResult<IReadOnlyCollection<Province>>(
                    SpecificationConverter<Province>.GetQuery(_provinces.AsQueryable(), specification).ToList()));
        _provinceRepository = provinces.Object;

        var localities = new Mock<ILocalityRepository>();
        localities
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<Locality>>(), It.IsAny<CancellationToken>()))
            .Returns<ISpecification<Locality>, CancellationToken>((specification, _) =>
                Task.FromResult<IReadOnlyCollection<Locality>>(
                    SpecificationConverter<Locality>.GetQuery(_localities.AsQueryable(), specification).ToList()));
        _localityRepository = localities.Object;

        var postalCodes = new Mock<IPostalCodeRepository>();
        postalCodes
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<PostalCode>>(), It.IsAny<CancellationToken>()))
            .Returns<ISpecification<PostalCode>, CancellationToken>((specification, _) =>
                Task.FromResult<IReadOnlyCollection<PostalCode>>(
                    SpecificationConverter<PostalCode>.GetQuery(_postalCodes.AsQueryable(), specification).ToList()));
        _postalCodeRepository = postalCodes.Object;
    }

    private Province AddProvince(string name, Guid? id = null)
    {
        var province = Province.Create(id ?? Guid.NewGuid(), name).Value;
        _provinces.Add(province);
        return province;
    }

    private Locality AddLocality(Guid provinceId, string name)
    {
        var locality = Locality.Create(Guid.NewGuid(), provinceId, name).Value;
        _localities.Add(locality);
        return locality;
    }

    [Test]
    public async Task ProvinceFinderAll_ReturnsEveryProvinceOrderedByNameIgnoringAccents()
    {
        AddProvince("Sevilla");
        AddProvince("Álava");
        AddProvince("Almería");

        var result = await new ProvinceFinderAll.Handler(_provinceRepository)
            .Handle(new ProvinceFinderAll.Query(), CancellationToken.None);

        Assert.That(result.Select(province => province.Name), Is.EqualTo(new[] { "Álava", "Almería", "Sevilla" }));
    }

    [Test]
    public async Task ProvinceFinderByIds_ReturnsOnlyTheExistingOnes()
    {
        var known = AddProvince("Almería");
        AddProvince("Madrid");

        var result = await new ProvinceFinderByIds.Handler(_provinceRepository)
            .Handle(new ProvinceFinderByIds.Query([known.Id, Guid.NewGuid()]), CancellationToken.None);

        Assert.That(result.Select(province => province.Id), Is.EqualTo(new[] { known.Id }));
    }

    [Test]
    public async Task LocalityFinderByProvince_ReturnsOnlyThatProvinceOrderedByName()
    {
        AddLocality(Almeria, "Níjar");
        AddLocality(Almeria, "Abla");
        AddLocality(Madrid, "Getafe");
        var handler = new LocalityFinderByProvince.Handler(_localityRepository);

        var result = await handler.Handle(new LocalityFinderByProvince.Query(Almeria, null), CancellationToken.None);

        Assert.That(result.Select(locality => locality.Name), Is.EqualTo(new[] { "Abla", "Níjar" }));
        Assert.That(result.Select(locality => locality.ProvinceId), Is.All.EqualTo(Almeria));
    }

    [TestCase("nijar")]
    [TestCase("NÍJ")]
    [TestCase("  jar ")]
    public async Task LocalityFinderByProvince_WithName_MatchesIgnoringCaseAndAccents(string name)
    {
        AddLocality(Almeria, "Níjar");
        AddLocality(Almeria, "Abla");
        var handler = new LocalityFinderByProvince.Handler(_localityRepository);

        var result = await handler.Handle(new LocalityFinderByProvince.Query(Almeria, name), CancellationToken.None);

        Assert.That(result.Select(locality => locality.Name), Is.EqualTo(new[] { "Níjar" }));
    }

    [Test]
    public async Task LocalityFinderByProvince_WithUnknownProvince_ReturnsEmpty()
    {
        AddLocality(Almeria, "Abla");

        var result = await new LocalityFinderByProvince.Handler(_localityRepository)
            .Handle(new LocalityFinderByProvince.Query(Guid.NewGuid(), null), CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task LocalityFinderByIds_ReturnsOnlyTheExistingOnesWithTheirProvince()
    {
        var abla = AddLocality(Almeria, "Abla");
        AddLocality(Madrid, "Getafe");

        var result = await new LocalityFinderByIds.Handler(_localityRepository)
            .Handle(new LocalityFinderByIds.Query([abla.Id, Guid.NewGuid()]), CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.Single().ProvinceId, Is.EqualTo(Almeria));
    }

    [Test]
    public async Task PostalCodeFinderByIds_ReturnsTheFiveDigitCode()
    {
        var abla = AddLocality(Almeria, "Abla");
        var postalCode = PostalCode.Create(Guid.NewGuid(), abla.Id, "04510").Value;
        _postalCodes.Add(postalCode);

        var result = await new PostalCodeFinderByIds.Handler(_postalCodeRepository)
            .Handle(new PostalCodeFinderByIds.Query([postalCode.Id, Guid.NewGuid()]), CancellationToken.None);

        Assert.That(result.Single().Code, Is.EqualTo("04510"));
        Assert.That(result.Single().LocalityId, Is.EqualTo(abla.Id));
    }
}

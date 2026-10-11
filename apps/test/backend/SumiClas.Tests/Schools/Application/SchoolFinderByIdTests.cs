using Moq;
using SumiClas.Localities.Application;
using SumiClas.PostalCodes.Application;
using SumiClas.Provinces.Application;
using SumiClas.Schools.Application;
using SumiClas.Schools.Domain;
using SumiClas.Schools.Domain.Services;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Tests.Schools.Application;

[TestFixture]
public class SchoolFinderByIdTests
{
    private static readonly Guid MalagaProvince = Guid.Parse("a0000000-0000-4000-8000-000000000001");
    private static readonly Guid MadridProvince = Guid.Parse("a0000000-0000-4000-8000-000000000002");
    private static readonly Guid Fuengirola = Guid.Parse("7a1d5e90-2b3c-4d6e-8f10-a1b2c3d4e5f6");
    private static readonly Guid Madrid = Guid.Parse("11111111-2222-4333-8444-555566667777");
    private static readonly Guid FuengirolaPostalCode = Guid.Parse("c0000000-0000-4000-8000-000000000001");

    private List<School> _store = null!;
    private Mock<IQueryBus> _queryBus = null!;
    private ISchoolRepository _schools = null!;

    [SetUp]
    public void SetUp()
    {
        _store = [];
        var schools = new Mock<ISchoolRepository>();
        schools
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns<Guid, CancellationToken>((id, _) => Task.FromResult(_store.FirstOrDefault(school => school.Id == id)));
        schools
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<School>>(), It.IsAny<CancellationToken>()))
            .Returns<ISpecification<School>, CancellationToken>((specification, _) =>
                Task.FromResult<IReadOnlyCollection<School>>(
                    SpecificationConverter<School>.GetQuery(_store.AsQueryable(), specification).ToList()));
        _schools = schools.Object;

        _queryBus = new Mock<IQueryBus>();
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<LocalityFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<LocalityResponse>)
                [new LocalityResponse(Fuengirola, MalagaProvince, "Fuengirola"), new LocalityResponse(Madrid, MadridProvince, "Madrid")]);
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<ProvinceFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<ProvinceResponse>)
                [new ProvinceResponse(MalagaProvince, "Málaga"), new ProvinceResponse(MadridProvince, "Madrid")]);
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<PostalCodeFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<PostalCodeResponse>)
                [new PostalCodeResponse(FuengirolaPostalCode, Fuengirola, "29640")]);
    }

    private School AddSchool(string name, Guid localityId, Guid? postalCodeId = null)
    {
        var school = School.Create(Guid.NewGuid(), name, Guid.NewGuid().ToString("N"), localityId, postalCodeId, "Calle Mayor 1", "952000000").Value;
        _store.Add(school);
        return school;
    }

    [Test]
    public async Task ById_ReturnsTheSchoolWithTerritoryNamesAndPhone()
    {
        var school = AddSchool("CEIP Sumi", Fuengirola, FuengirolaPostalCode);
        var handler = new SchoolFinderById.Finder(new SchoolFinder(_schools), _queryBus.Object);

        var result = await handler.ExecuteAsync(new SchoolFinderById.Query(school.Id), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        var response = result.Value;
        Assert.That(response.Name, Is.EqualTo("CEIP Sumi"));
        Assert.That(response.ProvinceName, Is.EqualTo("Málaga"));
        Assert.That(response.LocalityName, Is.EqualTo("Fuengirola"));
        Assert.That(response.PostalCode, Is.EqualTo("29640"));
        Assert.That(response.Address, Is.EqualTo("Calle Mayor 1"));
        Assert.That(response.Phone, Is.EqualTo("952000000"));
    }

    [Test]
    public async Task ById_WithUnknownSchool_ReturnsNotFoundWithoutAskingOtherModules()
    {
        var handler = new SchoolFinderById.Finder(new SchoolFinder(_schools), _queryBus.Object);

        var result = await handler.ExecuteAsync(new SchoolFinderById.Query(Guid.NewGuid()), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SchoolErrors.NotFound));
        _queryBus.Verify(bus => bus.Send(It.IsAny<LocalityFinderByIds.Query>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ByIds_ResolvesSchoolsOfDifferentProvincesWithOneQueryPerModule()
    {
        var first = AddSchool("CEIP Sumi", Fuengirola, FuengirolaPostalCode);
        var second = AddSchool("IES Centro", Madrid);
        var handler = new SchoolFinderByIds.Finder(_schools, _queryBus.Object);

        var result = await handler.ExecuteAsync(new SchoolFinderByIds.Query([first.Id, second.Id]), CancellationToken.None);

        Assert.That(result.Select(school => school.ProvinceName), Is.EquivalentTo(new[] { "Málaga", "Madrid" }));
        Assert.That(result.Single(school => school.Id == second.Id).PostalCode, Is.Null);
        _queryBus.Verify(bus => bus.Send(It.IsAny<LocalityFinderByIds.Query>(), It.IsAny<CancellationToken>()), Times.Once);
        _queryBus.Verify(bus => bus.Send(It.IsAny<ProvinceFinderByIds.Query>(), It.IsAny<CancellationToken>()), Times.Once);
        _queryBus.Verify(bus => bus.Send(It.IsAny<PostalCodeFinderByIds.Query>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ByIds_WithNoMatchingSchool_ReturnsEmpty()
    {
        var handler = new SchoolFinderByIds.Finder(_schools, _queryBus.Object);

        var result = await handler.ExecuteAsync(new SchoolFinderByIds.Query([Guid.NewGuid()]), CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ById_WhenTheLocalityIsMissingFromTheCatalog_FailsLoudly()
    {
        var school = AddSchool("CEIP Sumi", Guid.NewGuid());
        var handler = new SchoolFinderById.Finder(new SchoolFinder(_schools), _queryBus.Object);

        Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.ExecuteAsync(new SchoolFinderById.Query(school.Id), CancellationToken.None));
    }

    [Test]
    public async Task SchoolFinderByIdHandler_DelegatesToTheUseCase()
    {
        var handler = new SchoolFinderById.Handler(new SchoolFinderById.Finder(new SchoolFinder(_schools), _queryBus.Object));

        var result = await handler.Handle(new SchoolFinderById.Query(Guid.NewGuid()), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(SchoolErrors.NotFound));
    }

    [Test]
    public async Task SchoolFinderByIdsHandler_DelegatesToTheUseCase()
    {
        var handler = new SchoolFinderByIds.Handler(new SchoolFinderByIds.Finder(_schools, _queryBus.Object));

        var result = await handler.Handle(new SchoolFinderByIds.Query([Guid.NewGuid()]), CancellationToken.None);

        Assert.That(result, Is.Empty);
    }
}

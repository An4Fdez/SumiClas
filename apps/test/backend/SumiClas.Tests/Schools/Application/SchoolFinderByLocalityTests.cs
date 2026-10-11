using Moq;
using SumiClas.Localities.Application;
using SumiClas.PostalCodes.Application;
using SumiClas.Provinces.Application;
using SumiClas.Schools.Application;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Tests.Schools.Application;

[TestFixture]
public class SchoolFinderByLocalityTests
{
    private static readonly Guid MalagaProvince = Guid.Parse("a0000000-0000-4000-8000-000000000001");
    private static readonly Guid Malaga = Guid.Parse("7a1d5e90-2b3c-4d6e-8f10-a1b2c3d4e5f6");
    private static readonly Guid Madrid = Guid.Parse("11111111-2222-4333-8444-555566667777");
    private static readonly Guid MalagaPostalCode = Guid.Parse("c0000000-0000-4000-8000-000000000001");

    private List<School> _store = null!;
    private Mock<ISchoolRepository> _schools = null!;
    private Mock<IQueryBus> _queryBus = null!;
    private SchoolFinderByLocality.Finder _useCase = null!;

    [SetUp]
    public void SetUp()
    {
        _store = [];
        _schools = new Mock<ISchoolRepository>();
        _schools
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<School>>(), It.IsAny<CancellationToken>()))
            .Returns<ISpecification<School>, CancellationToken>((specification, _) =>
                Task.FromResult<IReadOnlyCollection<School>>(
                    SpecificationConverter<School>.GetQuery(_store.AsQueryable(), specification).ToList()));

        _queryBus = new Mock<IQueryBus>();
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<LocalityFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<LocalityResponse>)
                [new LocalityResponse(Malaga, MalagaProvince, "Fuengirola"), new LocalityResponse(Madrid, Guid.NewGuid(), "Madrid")]);
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<ProvinceFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<ProvinceResponse>) [new ProvinceResponse(MalagaProvince, "Málaga")]);
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<PostalCodeFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<PostalCodeResponse>) [new PostalCodeResponse(MalagaPostalCode, Malaga, "29640")]);

        _useCase = new SchoolFinderByLocality.Finder(_schools.Object, _queryBus.Object);
    }

    private School AddSchool(string name, Guid localityId, Guid? postalCodeId = null)
    {
        var school = School.Create(Guid.NewGuid(), name, Guid.NewGuid().ToString("N"), localityId, postalCodeId).Value;
        _store.Add(school);
        return school;
    }

    [Test]
    public async Task UseCase_ReturnsOnlySchoolsOfThatLocality()
    {
        var inMalaga = AddSchool("CEIP Sumi", Malaga);
        AddSchool("CEIP Sumi", Madrid);

        var result = (await _useCase.ExecuteAsync(new SchoolFinderByLocality.Query(Malaga, null), CancellationToken.None)).Value;

        Assert.That(result.Select(school => school.Id), Is.EqualTo(new[] { inMalaga.Id }));
        Assert.That(result.Single().LocalityId, Is.EqualTo(Malaga));
    }

    [Test]
    public async Task UseCase_ResolvesProvinceLocalityAndPostalCodeNames()
    {
        AddSchool("CEIP Sumi", Malaga, MalagaPostalCode);

        var school = (await _useCase.ExecuteAsync(new SchoolFinderByLocality.Query(Malaga, null), CancellationToken.None)).Value.Single();

        Assert.That(school.ProvinceId, Is.EqualTo(MalagaProvince));
        Assert.That(school.ProvinceName, Is.EqualTo("Málaga"));
        Assert.That(school.LocalityName, Is.EqualTo("Fuengirola"));
        Assert.That(school.PostalCodeId, Is.EqualTo(MalagaPostalCode));
        Assert.That(school.PostalCode, Is.EqualTo("29640"));
    }

    [Test]
    public async Task UseCase_WithoutPostalCode_ReturnsNullCodeAndDoesNotAskForPostalCodes()
    {
        AddSchool("CEIP Sumi", Malaga);

        var school = (await _useCase.ExecuteAsync(new SchoolFinderByLocality.Query(Malaga, null), CancellationToken.None)).Value.Single();

        Assert.That(school.PostalCodeId, Is.Null);
        Assert.That(school.PostalCode, Is.Null);
        _queryBus.Verify(bus => bus.Send(It.IsAny<PostalCodeFinderByIds.Query>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UseCase_WithManySchools_AsksEachModuleOnlyOnce()
    {
        for (var i = 0; i < 10; i++)
        {
            AddSchool($"CEIP {i:00}", Malaga, MalagaPostalCode);
        }

        await _useCase.ExecuteAsync(new SchoolFinderByLocality.Query(Malaga, null), CancellationToken.None);

        _queryBus.Verify(bus => bus.Send(It.IsAny<LocalityFinderByIds.Query>(), It.IsAny<CancellationToken>()), Times.Once);
        _queryBus.Verify(bus => bus.Send(It.IsAny<ProvinceFinderByIds.Query>(), It.IsAny<CancellationToken>()), Times.Once);
        _queryBus.Verify(bus => bus.Send(It.IsAny<PostalCodeFinderByIds.Query>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UseCase_WithName_MatchesIgnoringCaseAndAccents()
    {
        var match = AddSchool("CEIP Peña Blanca", Malaga);
        AddSchool("IES Los Álamos", Malaga);

        var result = (await _useCase.ExecuteAsync(new SchoolFinderByLocality.Query(Malaga, "  PENA bla "), CancellationToken.None)).Value;

        Assert.That(result.Select(school => school.Id), Is.EqualTo(new[] { match.Id }));
    }

    [Test]
    public async Task UseCase_WithAccentedSearch_MatchesUnaccentedName()
    {
        var match = AddSchool("CEIP Alamos", Malaga);

        var result = (await _useCase.ExecuteAsync(new SchoolFinderByLocality.Query(Malaga, "Álamos"), CancellationToken.None)).Value;

        Assert.That(result.Select(school => school.Id), Is.EqualTo(new[] { match.Id }));
    }

    [Test]
    public async Task UseCase_WithBlankName_ReturnsEverySchoolOfTheLocalityOrderedByName()
    {
        AddSchool("IES Zeta", Malaga);
        AddSchool("CEIP Alfa", Malaga);

        var result = (await _useCase.ExecuteAsync(new SchoolFinderByLocality.Query(Malaga, "  "), CancellationToken.None)).Value;

        Assert.That(result.Select(school => school.Name), Is.EqualTo(new[] { "CEIP Alfa", "IES Zeta" }));
    }

    [Test]
    public async Task UseCase_WithUnknownLocality_ReturnsNothingWithoutAskingOtherModules()
    {
        AddSchool("CEIP Sumi", Malaga);

        var result = (await _useCase.ExecuteAsync(new SchoolFinderByLocality.Query(Guid.NewGuid(), null), CancellationToken.None)).Value;

        Assert.That(result, Is.Empty);
        _queryBus.Verify(bus => bus.Send(It.IsAny<LocalityFinderByIds.Query>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UseCase_ReturnsAtMostMaxResultsTakingTheFirstByName()
    {
        for (var i = 0; i < SchoolFinderByLocality.MaxResults + 5; i++)
        {
            AddSchool($"CEIP {i:000}", Malaga);
        }

        var result = (await _useCase.ExecuteAsync(new SchoolFinderByLocality.Query(Malaga, null), CancellationToken.None)).Value;

        Assert.That(result, Has.Count.EqualTo(SchoolFinderByLocality.MaxResults));
        Assert.That(result.First().Name, Is.EqualTo("CEIP 000"));
        Assert.That(result.Last().Name, Is.EqualTo($"CEIP {SchoolFinderByLocality.MaxResults - 1:000}"));
    }


    [TestCase(null)]
    [TestCase("00000000-0000-0000-0000-000000000000")]
    public async Task UseCase_WithoutLocality_FailsWithoutTouchingRepositoryOrBus(string? localityId)
    {
        Guid? id = localityId is null ? null : Guid.Parse(localityId);

        var result = await _useCase.ExecuteAsync(new SchoolFinderByLocality.Query(id, null), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SchoolErrors.LocalityFilterRequired));
        _schools.Verify(repository => repository.SearchAsync(It.IsAny<ISpecification<School>>(), It.IsAny<CancellationToken>()), Times.Never);
        _queryBus.VerifyNoOtherCalls();
    }
    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var inMalaga = AddSchool("CEIP Sumi", Malaga);
        AddSchool("CEIP Sumi", Madrid);
        var handler = new SchoolFinderByLocality.Handler(_useCase);

        var result = (await handler.Handle(new SchoolFinderByLocality.Query(Malaga, null), CancellationToken.None)).Value;

        Assert.That(result.Select(school => school.Id), Is.EqualTo(new[] { inMalaga.Id }));
    }
}

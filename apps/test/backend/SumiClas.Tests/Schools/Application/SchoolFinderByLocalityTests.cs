using Moq;
using SumiClas.Schools.Application;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Domain;

namespace SumiClas.Tests.Schools.Application;

[TestFixture]
public class SchoolFinderByLocalityTests
{
    private static readonly Guid Malaga = Guid.Parse("7a1d5e90-2b3c-4d6e-8f10-a1b2c3d4e5f6");
    private static readonly Guid Madrid = Guid.Parse("11111111-2222-4333-8444-555566667777");

    private List<School> _store = null!;
    private SchoolFinderByLocality.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _store = [];
        var schools = new Mock<ISchoolRepository>();
        schools
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<School>>(), It.IsAny<CancellationToken>()))
            .Returns<ISpecification<School>, CancellationToken>((specification, _) =>
                Task.FromResult<IReadOnlyCollection<School>>(_store.Where(specification.Criteria.Compile()).ToList()));
        _handler = new SchoolFinderByLocality.Handler(schools.Object);
    }

    private School AddSchool(string name, Guid localityId)
    {
        var school = School.Create(Guid.NewGuid(), name, Guid.NewGuid().ToString("N"), localityId).Value;
        _store.Add(school);
        return school;
    }

    [Test]
    public async Task Handle_ReturnsOnlySchoolsOfThatLocality()
    {
        var inMalaga = AddSchool("CEIP Sumi", Malaga);
        AddSchool("CEIP Sumi", Madrid);

        var result = await _handler.Handle(new SchoolFinderByLocality.Query(Malaga, null), CancellationToken.None);

        Assert.That(result.Select(school => school.Id), Is.EqualTo(new[] { inMalaga.Id }));
        Assert.That(result.Single().LocalityId, Is.EqualTo(Malaga));
    }

    [Test]
    public async Task Handle_WithName_MatchesIgnoringCaseAndAccents()
    {
        var match = AddSchool("CEIP Peña Blanca", Malaga);
        AddSchool("IES Los Álamos", Malaga);

        var result = await _handler.Handle(new SchoolFinderByLocality.Query(Malaga, "  PENA bla "), CancellationToken.None);

        Assert.That(result.Select(school => school.Id), Is.EqualTo(new[] { match.Id }));
    }

    [Test]
    public async Task Handle_WithBlankName_ReturnsEverySchoolOfTheLocalityOrderedByName()
    {
        AddSchool("IES Zeta", Malaga);
        AddSchool("CEIP Alfa", Malaga);

        var result = await _handler.Handle(new SchoolFinderByLocality.Query(Malaga, "  "), CancellationToken.None);

        Assert.That(result.Select(school => school.Name), Is.EqualTo(new[] { "CEIP Alfa", "IES Zeta" }));
    }

    [Test]
    public async Task Handle_WithUnknownLocality_ReturnsNothing()
    {
        AddSchool("CEIP Sumi", Malaga);

        var result = await _handler.Handle(new SchoolFinderByLocality.Query(Guid.NewGuid(), null), CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task Handle_ReturnsAtMostMaxResults()
    {
        for (var i = 0; i < SchoolFinderByLocality.MaxResults + 5; i++)
        {
            AddSchool($"CEIP {i:000}", Malaga);
        }

        var result = await _handler.Handle(new SchoolFinderByLocality.Query(Malaga, null), CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(SchoolFinderByLocality.MaxResults));
    }
}

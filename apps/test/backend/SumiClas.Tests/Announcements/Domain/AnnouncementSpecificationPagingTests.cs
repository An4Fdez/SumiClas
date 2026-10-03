using Moq;
using SumiClas.Announcements.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Tests.Announcements.Domain;

[TestFixture]
public class AnnouncementSpecificationPagingTests
{
    private static readonly DateTimeOffset Start = new(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

    private static Announcement Build(int minutes)
    {
        var clock = new Mock<IClock>();
        clock.Setup(c => c.UtcNow).Returns(Start.AddMinutes(minutes));

        return SumiClas.Announcements.Domain.Announcement.Create(
            Guid.NewGuid(), AnnouncementScope.School, null, $"Aviso {minutes}", "Cuerpo", Priority.Normal, Guid.NewGuid(), null, clock.Object).Value;
    }

    [Test]
    public void FirstPage_OrdersNewestFirstAndTakesThePageSize()
    {
        var all = new[] { Build(1), Build(3), Build(2), Build(4) };

        var spec = new AnnouncementSpecification(page: new KeysetPage<DateTimeOffset>(3));
        var result = SpecificationConverter<Announcement>.GetQuery(all.AsQueryable(), spec).ToList();

        Assert.That(result.Select(item => item.Title), Is.EqualTo(new[] { "Aviso 4", "Aviso 3", "Aviso 2" }));
    }

    [Test]
    public void NextPage_KeepsOnlyWhatComesAfterTheBoundary()
    {
        var all = new[] { Build(1), Build(3), Build(2), Build(4) };
        var boundary = new KeysetBoundary<DateTimeOffset>(Start.AddMinutes(3), Guid.Empty);

        var spec = new AnnouncementSpecification(page: new KeysetPage<DateTimeOffset>(10, boundary));
        var result = SpecificationConverter<Announcement>.GetQuery(all.AsQueryable(), spec).ToList();

        Assert.That(result.Select(item => item.Title), Is.EqualTo(new[] { "Aviso 2", "Aviso 1" }));
    }

    [Test]
    public void NextPage_WithTiedKeys_UsesTheIdToBreakTheTie()
    {
        var first = Build(5);
        var second = Build(5);
        var ordered = new[] { first, second }.OrderByDescending(item => item.Id).ToList();
        var boundary = new KeysetBoundary<DateTimeOffset>(Start.AddMinutes(5), ordered[0].Id);

        var spec = new AnnouncementSpecification(page: new KeysetPage<DateTimeOffset>(10, boundary));
        var result = SpecificationConverter<Announcement>.GetQuery(new[] { first, second }.AsQueryable(), spec).ToList();

        Assert.That(result.Select(item => item.Id), Is.EqualTo(new[] { ordered[1].Id }));
    }
}

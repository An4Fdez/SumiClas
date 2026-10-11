using Moq;
using SumiClas.Announcements.Application;
using SumiClas.Announcements.Domain;
using SumiClas.CalendarEvents.Application;
using SumiClas.Classes.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Tests.Announcements.Application;

[TestFixture]
public class AnnouncementFinderByClassTests
{
    private static readonly Guid ClassId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AuthorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly DateTimeOffset Start = new(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

    private Mock<IAnnouncementRepository> _announcements = null!;
    private Mock<IQueryBus> _queryBus = null!;
    private Mock<IUserProvider> _userProvider = null!;
    private AnnouncementFinderByClass.Finder _useCase = null!;

    [SetUp]
    public void SetUp()
    {
        _announcements = new Mock<IAnnouncementRepository>();
        _queryBus = new Mock<IQueryBus>();
        _userProvider = new Mock<IUserProvider>();

        _userProvider.Setup(provider => provider.GetCurrentUserId()).Returns(Result<Guid>.Success(AuthorId));
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<Result<ClassResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Success(new ClassResponse(ClassId, Guid.NewGuid(), "3A")));
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<IReadOnlyCollection<UserClassMemberResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserClassMemberResponse>());
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<IReadOnlyCollection<CalendarEventSummaryResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalendarEventSummaryResponse>());

        _useCase = new AnnouncementFinderByClass.Finder(_announcements.Object, _queryBus.Object, _userProvider.Object);
    }

    private static Announcement Build(AnnouncementScope scope, int minutes)
    {
        var clock = new Mock<IClock>();
        clock.Setup(c => c.UtcNow).Returns(Start.AddMinutes(minutes));

        return SumiClas.Announcements.Domain.Announcement.Create(
            Guid.NewGuid(),
            scope,
            scope == AnnouncementScope.Class ? ClassId : null,
            $"Aviso {minutes}",
            "Cuerpo",
            Priority.Normal,
            AuthorId,
            null,
            clock.Object).Value;
    }

    private void ReturnBranches(IReadOnlyCollection<Announcement> school, IReadOnlyCollection<Announcement> schoolClass)
    {
        _announcements
            .SetupSequence(repository => repository.SearchAsync(It.IsAny<ISpecification<Announcement>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(school)
            .ReturnsAsync(schoolClass);
    }

    [Test]
    public async Task UseCase_WithMoreAnnouncementsThanLimit_MergesBothBranchesNewestFirstAndReturnsNextCursor()
    {
        ReturnBranches(
            [Build(AnnouncementScope.School, 3), Build(AnnouncementScope.School, 1)],
            [Build(AnnouncementScope.Class, 4), Build(AnnouncementScope.Class, 2)]);

        var result = await _useCase.ExecuteAsync(new AnnouncementFinderByClass.Query(ClassId, 2), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Items.Select(item => item.Title), Is.EqualTo(new[] { "Aviso 4", "Aviso 3" }));
        Assert.That(PageCursor.TryDecode(result.Value.NextCursor, out var cursor), Is.True);
        Assert.That(cursor.Key, Is.EqualTo(Start.AddMinutes(3).UtcTicks));
    }

    [Test]
    public async Task UseCase_WithAsManyAnnouncementsAsLimit_HasNoNextCursor()
    {
        ReturnBranches(
            [Build(AnnouncementScope.School, 1)],
            [Build(AnnouncementScope.Class, 2)]);

        var result = await _useCase.ExecuteAsync(new AnnouncementFinderByClass.Query(ClassId, 2), CancellationToken.None);

        Assert.That(result.Value.Items, Has.Count.EqualTo(2));
        Assert.That(result.Value.NextCursor, Is.Null);
    }

    [Test]
    public async Task UseCase_WithInvalidCursor_FailsBeforeReadingAnything()
    {
        var result = await _useCase.ExecuteAsync(new AnnouncementFinderByClass.Query(ClassId, 10, "garbage!"), CancellationToken.None);

        Assert.That(result.Error!.Code, Is.EqualTo("Pagination.InvalidCursor"));
        _announcements.Verify(
            repository => repository.SearchAsync(It.IsAny<ISpecification<Announcement>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task UseCase_WithLimitOutOfRange_ReturnsInvalidLimit()
    {
        var result = await _useCase.ExecuteAsync(new AnnouncementFinderByClass.Query(ClassId, 500), CancellationToken.None);

        Assert.That(result.Error!.Code, Is.EqualTo("Pagination.InvalidLimit"));
    }

    [Test]
    public async Task UseCase_WithAnUnknownClass_ReturnsTheClassError()
    {
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<Result<ClassResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Failure(new Error("Class.NotFound", "No existe.")));

        var result = await _useCase.ExecuteAsync(new AnnouncementFinderByClass.Query(ClassId), CancellationToken.None);

        Assert.That(result.Error!.Code, Is.EqualTo("Class.NotFound"));
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var handler = new AnnouncementFinderByClass.Handler(_useCase);

        var result = await handler.Handle(new AnnouncementFinderByClass.Query(ClassId, 500), CancellationToken.None);

        Assert.That(result.Error!.Code, Is.EqualTo("Pagination.InvalidLimit"));
    }
}

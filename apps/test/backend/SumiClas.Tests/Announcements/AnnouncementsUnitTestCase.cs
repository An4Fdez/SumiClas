namespace SumiClas.Tests.Announcements;

using Moq;
using SumiClas.Announcements.Domain;
using SumiClas.CalendarEvents.Application;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.CalendarEvents.Application;
using SumiClas.Tests.Classes.Application;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

public abstract class AnnouncementsUnitTestCase : UnitTestCase
{
    private readonly List<Announcement> _added = [];

    protected Mock<IAnnouncementRepository> Repository { get; } = new();

    protected IReadOnlyList<Announcement> Added => this._added;

    [SetUp]
    public void AnnouncementsSetUp()
    {
        this._added.Clear();
        this.Repository.Reset();
    }

    protected void ShouldCaptureAddedAnnouncements()
        => this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<Announcement>(), It.IsAny<CancellationToken>()))
            .Callback<Announcement, CancellationToken>((announcement, _) => this._added.Add(announcement))
            .Returns(Task.CompletedTask);

    protected void ShouldFetchAnnouncements(params Announcement[] announcements)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => announcements.FirstOrDefault(announcement => announcement.Id == id));

    protected void ShouldSearchAnnouncements(params Announcement[] announcements)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Announcement>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Announcement> specification, CancellationToken _)
                => InMemorySearch.Run(announcements, specification));

    protected void ShouldSearchAnnouncementsSuccessively(
        IReadOnlyCollection<Announcement> first,
        IReadOnlyCollection<Announcement> second)
        => this.Repository
            .SetupSequence(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Announcement>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(first)
            .ReturnsAsync(second);

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<Announcement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldNotHaveRead()
        => this.Repository.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldFindUser(Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(UserResponseMother.With(userId, StringMother.Random("name"))));

    protected void ShouldNotFindUser(Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure(UserErrors.NotFound));

    protected void ShouldFindClassInSchoolYear(Guid classId, Guid schoolYearId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassFinderById.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Success(ClassResponseMother.With(classId, schoolYearId)));

    protected void ShouldFindClass(Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassFinderById.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Success(ClassResponseMother.WithId(classId)));

    protected void ShouldNotFindClass()
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<Result<ClassResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Failure(ClassErrors.NotFound));

    protected void ShouldEnsureSchoolYearIsActive(Guid schoolYearId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolYearEnsureIsActive.Query>(query => query.SchoolYearId == schoolYearId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

    protected void ShouldFailEnsuringSchoolYearIsActive(Guid schoolYearId, Error error)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolYearEnsureIsActive.Query>(query => query.SchoolYearId == schoolYearId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

    protected void ShouldFindCalendarEvent(Guid calendarEventId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<CalendarEventFinderById.Query>(query => query.CalendarEventId == calendarEventId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CalendarEventSummaryResponse>.Success(
                CalendarEventSummaryResponseMother.With(calendarEventId, StringMother.Random("Event"))));

    protected void ShouldNotFindCalendarEvent(Guid calendarEventId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<CalendarEventFinderById.Query>(query => query.CalendarEventId == calendarEventId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CalendarEventSummaryResponse>.Failure(CalendarEventErrors.NotFound));
}

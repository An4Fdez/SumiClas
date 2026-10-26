namespace SumiClas.Tests.Announcements.Application;

using AwesomeAssertions;
using SumiClas.Announcements.Application;
using SumiClas.Announcements.Domain;
using SumiClas.CalendarEvents.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Announcements.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class AnnouncementUpdateShould : AnnouncementsUnitTestCase
{
    private static readonly TimeSpan ONE_HOUR = TimeSpan.FromHours(1);

    private readonly AnnouncementUpdate.Update _useCase;

    public AnnouncementUpdateShould()
    {
        this._useCase = new AnnouncementUpdate.Update(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task change_the_content_of_the_announcement_of_the_current_user_and_leave_it_unread_for_everybody()
    {
        this.ShouldCaptureAddedAnnouncements();
        Guid authorId = GuidMother.Random();
        Guid announcementId = GuidMother.Random();
        Guid eventId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.With(
            Now - ONE_HOUR,
            id: announcementId,
            scope: AnnouncementScope.Class,
            classId: classId,
            authorId: authorId);
        announcement.MarkAsReadBy(GuidMother.Random(), this.Clock.Object);
        announcement.MarkAsReadBy(authorId, this.Clock.Object);
        Announcement other = AnnouncementMother.ReadBy(GuidMother.Random(), GuidMother.Random(), Now);
        string otherTitle = other.Title;
        this.ShouldFetchAnnouncements(other, announcement);
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindCalendarEvent(eventId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.AdvanceClock(ONE_HOUR);
        string newTitle = StringMother.Random("New title");
        string newBody = StringMother.Random("New body");

        await this._useCase.ExecuteAsync(
            AnnouncementUpdateCommandMother.Valid(
                announcementId,
                title: $"  {newTitle}  ",
                body: $"  {newBody}  ",
                priority: "High",
                relatedEventId: eventId));

        announcement.Title.Should().Be(newTitle);
        announcement.Body.Should().Be(newBody);
        announcement.Priority.Should().Be(Priority.High);
        announcement.RelatedEventId.Should().Be(eventId);
        announcement.UpdatedAt.Should().Be(Now + ONE_HOUR);
        announcement.CreatedAt.Should().Be(Now - ONE_HOUR);
        announcement.ReadReceipts.Should().BeEmpty();
        other.Title.Should().Be(otherTitle);
        other.UpdatedAt.Should().BeNull();
        other.ReadReceipts.Should().ContainSingle();
        this.Added.Should().BeEmpty();
        this.ShouldNotHaveRemoved();
        this.ShouldHaveAskedForCalendarEvent(eventId);
        this.ShouldHaveEnsuredSchoolYearIsActive(schoolYearId);
    }

    [Test]
    public async Task clear_the_related_event_without_asking_for_a_calendar_event_when_the_command_has_none()
    {
        Guid authorId = GuidMother.Random();
        Guid announcementId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.With(
            Now,
            id: announcementId,
            authorId: authorId,
            relatedEventId: GuidMother.Random());
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldResolveCurrentUser(authorId);

        await this._useCase.ExecuteAsync(AnnouncementUpdateCommandMother.Valid(announcementId));

        announcement.RelatedEventId.Should().BeNull();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase("low", Priority.Low)]
    [TestCase("HIGH", Priority.High)]
    public async Task accept_the_priority_whatever_its_casing(string priority, Priority expected)
    {
        Guid authorId = GuidMother.Random();
        Guid announcementId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.With(Now, id: announcementId, authorId: authorId);
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldResolveCurrentUser(authorId);

        await this._useCase.ExecuteAsync(AnnouncementUpdateCommandMother.Valid(announcementId, priority: priority));

        announcement.Priority.Should().Be(expected);
    }

    [Test]
    public async Task throw_unauthenticated_without_reading_or_asking_when_there_is_no_current_user()
    {
        Announcement announcement = AnnouncementMother.WithId(GuidMother.Random());
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementUpdateCommandMother.Valid(announcement.Id));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.ShouldNotHaveRead();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_not_found_without_asking_when_the_announcement_does_not_exist()
    {
        this.ShouldFetchAnnouncements(AnnouncementMother.WithId(GuidMother.Random()));
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementUpdateCommandMother.Valid(GuidMother.Random()));

        await act.Should().ThrowAsync<AnnouncementExceptions.NotFound>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_change_forbidden_without_changing_anything_when_the_current_user_is_not_the_author()
    {
        Guid announcementId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.ReadBy(
            GuidMother.Random(),
            GuidMother.Random(),
            Now,
            id: announcementId);
        string title = announcement.Title;
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementUpdateCommandMother.Valid(announcementId));

        await act.Should().ThrowAsync<AnnouncementExceptions.ChangeForbidden>();
        announcement.Title.Should().Be(title);
        announcement.UpdatedAt.Should().BeNull();
        announcement.ReadReceipts.Should().ContainSingle();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase("Urgent")]
    [TestCase("")]
    public async Task throw_invalid_priority_without_changing_anything_when_the_priority_is_not_a_known_one(
        string priority)
    {
        Guid authorId = GuidMother.Random();
        Guid announcementId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.ReadBy(
            GuidMother.Random(),
            GuidMother.Random(),
            Now,
            authorId: authorId,
            id: announcementId);
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldResolveCurrentUser(authorId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementUpdateCommandMother.Valid(announcementId, priority: priority));

        await act.Should().ThrowAsync<AnnouncementExceptions.InvalidPriority>();
        announcement.UpdatedAt.Should().BeNull();
        announcement.ReadReceipts.Should().ContainSingle();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_school_year_archived_without_changing_anything_when_the_year_of_the_class_is_archived()
    {
        Guid authorId = GuidMother.Random();
        Guid announcementId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.With(
            Now,
            id: announcementId,
            scope: AnnouncementScope.Class,
            classId: classId,
            authorId: authorId);
        announcement.MarkAsReadBy(GuidMother.Random(), this.Clock.Object);
        string title = announcement.Title;
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementUpdateCommandMother.Valid(announcementId));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        announcement.Title.Should().Be(title);
        announcement.UpdatedAt.Should().BeNull();
        announcement.ReadReceipts.Should().ContainSingle();
    }

    [Test]
    public async Task throw_calendar_event_not_found_without_changing_anything_when_the_related_event_does_not_exist()
    {
        Guid authorId = GuidMother.Random();
        Guid announcementId = GuidMother.Random();
        Guid eventId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.ReadBy(
            GuidMother.Random(),
            GuidMother.Random(),
            Now,
            authorId: authorId,
            id: announcementId);
        string title = announcement.Title;
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldNotFindCalendarEvent(eventId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementUpdateCommandMother.Valid(announcementId, relatedEventId: eventId));

        await act.Should().ThrowAsync<CalendarEventExceptions.NotFound>();
        announcement.Title.Should().Be(title);
        announcement.UpdatedAt.Should().BeNull();
        announcement.ReadReceipts.Should().ContainSingle();
    }
}

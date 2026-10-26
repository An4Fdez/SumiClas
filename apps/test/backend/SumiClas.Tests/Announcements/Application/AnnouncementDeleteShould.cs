namespace SumiClas.Tests.Announcements.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Announcements.Application;
using SumiClas.Announcements.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Announcements.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class AnnouncementDeleteShould : AnnouncementsUnitTestCase
{
    private readonly AnnouncementDelete.Delete _useCase;

    public AnnouncementDeleteShould()
    {
        this._useCase = new AnnouncementDelete.Delete(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task remove_only_the_requested_announcement_when_the_current_user_is_its_author()
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
        Announcement other = AnnouncementMother.With(Now, authorId: authorId);
        this.ShouldFetchAnnouncements(other, announcement);
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);

        await this._useCase.ExecuteAsync(AnnouncementDeleteCommandMother.WithAnnouncementId(announcementId));

        this.ShouldHaveRemoved(announcement);
        this.Repository.Verify(repository => repository.Remove(other), Times.Never);
        this.ShouldHaveEnsuredSchoolYearIsActive(schoolYearId);
    }

    [Test]
    public async Task throw_school_year_archived_without_removing_when_the_school_year_of_the_class_is_archived()
    {
        Guid authorId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.With(
            Now,
            scope: AnnouncementScope.Class,
            classId: classId,
            authorId: authorId);
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementDeleteCommandMother.WithAnnouncementId(announcement.Id));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        this.ShouldNotHaveRemoved();
    }

    [Test]
    public async Task throw_unauthenticated_without_reading_or_removing_when_there_is_no_current_user()
    {
        Announcement announcement = AnnouncementMother.WithId(GuidMother.Random());
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementDeleteCommandMother.WithAnnouncementId(announcement.Id));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.ShouldNotHaveRead();
        this.ShouldNotHaveRemoved();
    }

    [Test]
    public async Task throw_not_found_without_removing_when_the_announcement_does_not_exist()
    {
        this.ShouldFetchAnnouncements(AnnouncementMother.WithId(GuidMother.Random()));
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementDeleteCommandMother.WithAnnouncementId(GuidMother.Random()));

        await act.Should().ThrowAsync<AnnouncementExceptions.NotFound>();
        this.ShouldNotHaveRemoved();
    }

    [Test]
    public async Task throw_change_forbidden_without_removing_when_the_current_user_is_not_the_author()
    {
        Announcement announcement = AnnouncementMother.WithId(GuidMother.Random());
        this.ShouldFetchAnnouncements(announcement);
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementDeleteCommandMother.WithAnnouncementId(announcement.Id));

        await act.Should().ThrowAsync<AnnouncementExceptions.ChangeForbidden>();
        this.ShouldNotHaveRemoved();
    }
}

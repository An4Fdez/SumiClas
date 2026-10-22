namespace SumiClas.Tests.Announcements.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Announcements.Application;
using SumiClas.Announcements.Domain;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class AnnouncementCreateShould : AnnouncementsUnitTestCase
{
    private readonly AnnouncementCreate.Create _useCase;

    public AnnouncementCreateShould()
    {
        this._useCase = new AnnouncementCreate.Create(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_the_school_announcement_with_its_data_trimmed_and_the_author_and_clock_time()
    {
        Guid announcementId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        string title = StringMother.Random("notice");
        string body = StringMother.Random("body");
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldCaptureAddedAnnouncements();

        await this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.With(
            announcementId,
            "School",
            classId,
            "  " + title + " ",
            "\n" + body + "  ",
            "High",
            null));

        Announcement added = this.Added.Single();
        added.Id.Should().Be(announcementId);
        added.Scope.Should().Be(AnnouncementScope.School);
        added.ClassId.Should().BeNull();
        added.SchoolId.Should().Be(schoolId);
        added.Title.Should().Be(title);
        added.Body.Should().Be(body);
        added.Priority.Should().Be(Priority.High);
        added.AuthorId.Should().Be(authorId);
        added.RelatedEventId.Should().BeNull();
        added.CreatedAt.Should().Be(Now);
        added.ReadReceipts.Should().BeEmpty();
    }

    [Test]
    public async Task add_the_class_announcement_with_its_class_when_the_class_exists_and_the_school_year_is_active()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedAnnouncements();

        await this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(scope: "Class", classId: classId));

        Announcement added = this.Added.Single();
        added.Scope.Should().Be(AnnouncementScope.Class);
        added.ClassId.Should().Be(classId);
    }

    [Test]
    public async Task keep_the_related_event_when_it_exists()
    {
        Guid eventId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindCalendarEvent(eventId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedAnnouncements();

        await this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(classId: classId, relatedEventId: eventId));

        this.Added.Single().RelatedEventId.Should().Be(eventId);
        this.ShouldHaveAskedForCalendarEvent(eventId);
    }

    [TestCase("school", AnnouncementScope.School)]
    [TestCase("CLASS", AnnouncementScope.Class)]
    public async Task accept_the_scope_whatever_its_casing(string scope, AnnouncementScope expected)
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, GuidMother.Random());
        this.ShouldCaptureAddedAnnouncements();

        await this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.Valid(scope: scope, classId: classId));

        this.Added.Single().Scope.Should().Be(expected);
    }

    [TestCase("low", Priority.Low)]
    [TestCase("NORMAL", Priority.Normal)]
    public async Task accept_the_priority_whatever_its_casing(string priority, Priority expected)
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedAnnouncements();

        await this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.Valid(classId: classId, priority: priority));

        this.Added.Single().Priority.Should().Be(expected);
    }

    [Test]
    public async Task throw_unauthenticated_without_asking_or_adding_when_there_is_no_current_user()
    {
        this.ShouldCaptureAddedAnnouncements();
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(scope: StringMother.Random("scope")));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase("")]
    [TestCase("7")]
    public async Task throw_invalid_scope_without_asking_or_adding_when_the_scope_is_not_a_known_one(string scope)
    {
        this.ShouldCaptureAddedAnnouncements();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.Valid(scope: scope));

        await act.Should().ThrowAsync<AnnouncementExceptions.InvalidScope>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase("")]
    [TestCase("9")]
    public async Task throw_invalid_priority_without_asking_or_adding_when_the_priority_is_not_a_known_one(
        string priority)
    {
        this.ShouldCaptureAddedAnnouncements();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.Valid(priority: priority));

        await act.Should().ThrowAsync<AnnouncementExceptions.InvalidPriority>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_invalid_scope_when_both_the_scope_and_the_priority_are_invalid()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.Valid(
            scope: StringMother.Random("scope"),
            priority: StringMother.Random("priority")));

        await act.Should().ThrowAsync<AnnouncementExceptions.InvalidScope>();
    }

    [Test]
    public async Task throw_invalid_scope_when_the_scope_is_invalid_and_the_id_is_empty()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(id: Guid.Empty, scope: StringMother.Random("scope")));

        await act.Should().ThrowAsync<AnnouncementExceptions.InvalidScope>();
    }

    [Test]
    public async Task throw_invalid_priority_when_the_priority_is_invalid_and_the_id_is_empty()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(id: Guid.Empty, priority: StringMother.Random("priority")));

        await act.Should().ThrowAsync<AnnouncementExceptions.InvalidPriority>();
    }

    [Test]
    public async Task throw_id_required_without_asking_or_adding_when_the_id_is_empty()
    {
        this.ShouldCaptureAddedAnnouncements();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.Valid(id: Guid.Empty));

        await act.Should().ThrowAsync<AnnouncementExceptions.IdRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_class_required_without_asking_or_adding_when_the_class_scope_has_no_class()
    {
        this.ShouldCaptureAddedAnnouncements();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.ClassScopeWithClassId(null));

        await act.Should().ThrowAsync<AnnouncementExceptions.ClassRequiredForClassScope>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_class_required_without_asking_or_adding_when_the_class_scope_has_an_empty_class()
    {
        this.ShouldCaptureAddedAnnouncements();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.ClassScopeWithClassId(Guid.Empty));

        await act.Should().ThrowAsync<AnnouncementExceptions.ClassRequiredForClassScope>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_title_required_without_asking_or_adding_when_the_title_is_blank()
    {
        this.ShouldCaptureAddedAnnouncements();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.Valid(title: "   "));

        await act.Should().ThrowAsync<AnnouncementExceptions.TitleRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_body_required_without_asking_or_adding_when_the_body_is_blank()
    {
        this.ShouldCaptureAddedAnnouncements();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.Valid(body: "   "));

        await act.Should().ThrowAsync<AnnouncementExceptions.BodyRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_author_required_without_asking_or_adding_when_the_current_user_id_is_empty()
    {
        this.ShouldCaptureAddedAnnouncements();
        this.ShouldResolveCurrentUser(Guid.Empty);

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.Valid());

        await act.Should().ThrowAsync<AnnouncementExceptions.AuthorRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_id_required_when_the_id_is_empty_and_the_class_is_missing()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.ClassScopeWithClassId(null, id: Guid.Empty));

        await act.Should().ThrowAsync<AnnouncementExceptions.IdRequired>();
    }

    [Test]
    public async Task throw_class_required_when_the_class_is_missing_and_the_title_is_blank()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.ClassScopeWithClassId(null, title: "  "));

        await act.Should().ThrowAsync<AnnouncementExceptions.ClassRequiredForClassScope>();
    }

    [Test]
    public async Task throw_title_required_when_the_title_and_the_body_are_blank()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(title: "  ", body: "  "));

        await act.Should().ThrowAsync<AnnouncementExceptions.TitleRequired>();
    }

    [Test]
    public async Task throw_body_required_when_the_body_is_blank_and_the_current_user_id_is_empty()
    {
        this.ShouldResolveCurrentUser(Guid.Empty);

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.Valid(body: "  "));

        await act.Should().ThrowAsync<AnnouncementExceptions.BodyRequired>();
    }

    [Test]
    public async Task throw_user_not_found_without_asking_for_the_event_or_the_class_when_the_author_does_not_exist()
    {
        this.ShouldCaptureAddedAnnouncements();
        Guid authorId = GuidMother.Random();
        Guid eventId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldNotFindUser(authorId);

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.Valid(
            scope: "Class",
            classId: GuidMother.Random(),
            relatedEventId: eventId));

        await act.Should().ThrowAsync<UserExceptions.NotFound>();
        this.Added.Should().BeEmpty();
        this.ShouldHaveAskedOnlyForTheAuthor();
    }

    [Test]
    public async Task throw_calendar_event_not_found_without_asking_for_the_class_when_the_event_does_not_exist()
    {
        this.ShouldCaptureAddedAnnouncements();
        Guid eventId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldNotFindCalendarEvent(eventId);

        Func<Task> act = () => this._useCase.ExecuteAsync(AnnouncementCreateCommandMother.Valid(
            scope: "Class",
            classId: GuidMother.Random(),
            relatedEventId: eventId));

        await act.Should().ThrowAsync<CalendarEventExceptions.NotFound>();
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<ClassFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_class_not_found_without_asking_for_the_school_year_when_the_class_does_not_exist()
    {
        this.ShouldCaptureAddedAnnouncements();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldNotFindClass();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(scope: "Class", classId: GuidMother.Random()));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<SchoolYearEnsureIsActive.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_school_year_archived_without_adding_when_the_school_year_of_the_class_is_archived()
    {
        this.ShouldCaptureAddedAnnouncements();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(scope: "Class", classId: classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_school_year_not_found_without_adding_when_the_school_year_of_the_class_does_not_exist()
    {
        this.ShouldCaptureAddedAnnouncements();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.NotFound());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(scope: "Class", classId: classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.Added.Should().BeEmpty();
    }

    private void ShouldHaveAskedOnlyForTheAuthor()
    {
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<UserFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Once);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_class_representative_forbidden_without_adding_when_the_author_is_not_a_delegate()
    {
        this.ShouldCaptureAddedAnnouncements();
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldDenyClassRepresentative(authorId, classId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(scope: "Class", classId: classId));

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeForbidden>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_class_required_without_asking_or_adding_when_the_school_scope_has_no_class()
    {
        this.ShouldCaptureAddedAnnouncements();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.With(
                GuidMother.Random(),
                "School",
                null,
                StringMother.Random("notice"),
                StringMother.Random("body"),
                "Normal",
                null));

        await act.Should().ThrowAsync<AnnouncementExceptions.ClassRequiredForSchoolScope>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_school_year_not_found_without_adding_when_the_school_of_the_class_cannot_be_found()
    {
        this.ShouldCaptureAddedAnnouncements();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFailFindingSchoolOfSchoolYear(schoolYearId, new SchoolYearExceptions.NotFound());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(scope: "School", classId: classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_class_required_without_asking_or_adding_when_the_school_scope_has_an_empty_class()
    {
        this.ShouldCaptureAddedAnnouncements();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(scope: "School", classId: Guid.Empty));

        await act.Should().ThrowAsync<AnnouncementExceptions.ClassRequiredForSchoolScope>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_representative_forbidden_without_adding_the_school_announcement_when_not_a_delegate()
    {
        this.ShouldCaptureAddedAnnouncements();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldDenyClassRepresentative(authorId, classId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(scope: "School", classId: classId));

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeForbidden>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_school_year_archived_without_adding_the_school_announcement_when_the_class_is_archived()
    {
        this.ShouldCaptureAddedAnnouncements();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementCreateCommandMother.Valid(scope: "School", classId: classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        this.Added.Should().BeEmpty();
    }
}

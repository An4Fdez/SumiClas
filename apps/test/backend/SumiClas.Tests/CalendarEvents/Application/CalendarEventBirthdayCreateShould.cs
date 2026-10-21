namespace SumiClas.Tests.CalendarEvents.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.CalendarEvents.Application;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class CalendarEventBirthdayCreateShould : CalendarEventsUnitTestCase
{
    private readonly CalendarEventBirthdayCreate.Create _useCase;

    public CalendarEventBirthdayCreateShould()
    {
        this._useCase = new CalendarEventBirthdayCreate.Create(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_the_birthday_of_the_student_with_month_and_day_but_no_year_and_the_trimmed_title()
    {
        Guid eventId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string title = StringMother.Random("birthday");
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldEnsureStudentGuardian(studentId, authorId);
        this.ShouldCaptureAddedCalendarEvents();

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventBirthdayCreateCommandMother.With(eventId, studentId, "  " + title + " ", 3, 21));

        result.IsSuccess.Should().BeTrue();
        CalendarEvent added = this.Added.Single();
        added.Id.Should().Be(eventId);
        added.Scope.Should().Be(EventScope.Class);
        added.ClassId.Should().BeNull();
        added.StudentId.Should().Be(studentId);
        added.Type.Should().Be(EventType.Birthday);
        added.Title.Should().Be(title);
        added.EventDate.Year.Should().BeNull();
        added.EventDate.Month.Should().Be(3);
        added.EventDate.Day.Should().Be(21);
        added.Time.Should().BeNull();
        added.Location.Should().BeNull();
        added.Cost.Should().BeNull();
        added.AuthorId.Should().Be(authorId);
        added.CreatedAt.Should().Be(Now);
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<UserFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Once);
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<StudentGuardianEnsure.Query>(), It.IsAny<CancellationToken>()),
            Times.Once);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase(1, 31)]
    [TestCase(4, 30)]
    [TestCase(2, 29)]
    [TestCase(12, 31)]
    public async Task accept_the_last_valid_day_of_the_month_including_the_leap_day(int month, int day)
    {
        Guid studentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldEnsureStudentGuardian(studentId, authorId);
        this.ShouldCaptureAddedCalendarEvents();

        Result result = await this._useCase.ExecuteAsync(CalendarEventBirthdayCreateCommandMother.Valid(
            studentId: studentId,
            month: month,
            day: day));

        result.IsSuccess.Should().BeTrue();
        CalendarEvent added = this.Added.Single();
        added.EventDate.Month.Should().Be(month);
        added.EventDate.Day.Should().Be(day);
    }

    [Test]
    public async Task return_unauthenticated_without_asking_or_adding_when_there_is_no_current_user()
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldHaveNoCurrentUser();

        Result result = await this._useCase.ExecuteAsync(CalendarEventBirthdayCreateCommandMother.Valid(month: 13));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_id_required_without_asking_or_adding_when_the_id_is_empty()
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventBirthdayCreateCommandMother.Valid(id: Guid.Empty));

        result.Error.Should().Be(CalendarEventErrors.IdRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_student_required_without_asking_or_adding_when_the_student_id_is_empty()
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventBirthdayCreateCommandMother.Valid(studentId: Guid.Empty));

        result.Error.Should().Be(CalendarEventErrors.StudentRequiredForBirthday);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_title_required_without_asking_or_adding_when_the_title_is_blank()
    {
        string title = "   ";
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(CalendarEventBirthdayCreateCommandMother.Valid(title: title));

        result.Error.Should().Be(CalendarEventErrors.TitleRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_author_required_without_asking_or_adding_when_the_current_user_id_is_empty()
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(Guid.Empty);

        Result result = await this._useCase.ExecuteAsync(CalendarEventBirthdayCreateCommandMother.Valid());

        result.Error.Should().Be(CalendarEventErrors.AuthorRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase(0)]
    [TestCase(13)]
    [TestCase(-1)]
    public async Task return_invalid_month_without_asking_or_adding_when_the_month_is_out_of_range(int month)
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(CalendarEventBirthdayCreateCommandMother.Valid(month: month));

        result.Error.Should().Be(CalendarEventErrors.InvalidMonth);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase(1, 0)]
    [TestCase(1, 32)]
    [TestCase(4, 31)]
    [TestCase(2, 30)]
    public async Task return_invalid_day_without_asking_or_adding_when_the_day_does_not_exist_in_the_month(
        int month,
        int day)
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventBirthdayCreateCommandMother.Valid(month: month, day: day));

        result.Error.Should().Be(CalendarEventErrors.InvalidDay);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_invalid_month_when_both_the_month_and_the_day_are_invalid()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventBirthdayCreateCommandMother.Valid(month: 13, day: 40));

        result.Error.Should().Be(CalendarEventErrors.InvalidMonth);
    }

    [Test]
    public async Task return_id_required_when_the_id_and_the_student_are_empty()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventBirthdayCreateCommandMother.Valid(id: Guid.Empty, studentId: Guid.Empty));

        result.Error.Should().Be(CalendarEventErrors.IdRequired);
    }

    [Test]
    public async Task return_student_required_when_the_student_is_empty_and_the_title_is_blank()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventBirthdayCreateCommandMother.Valid(studentId: Guid.Empty, title: "  "));

        result.Error.Should().Be(CalendarEventErrors.StudentRequiredForBirthday);
    }

    [Test]
    public async Task return_title_required_when_the_title_is_blank_and_the_current_user_id_is_empty()
    {
        this.ShouldResolveCurrentUser(Guid.Empty);

        Result result = await this._useCase.ExecuteAsync(CalendarEventBirthdayCreateCommandMother.Valid(title: "  "));

        result.Error.Should().Be(CalendarEventErrors.TitleRequired);
    }

    [Test]
    public async Task return_author_required_when_the_current_user_id_is_empty_and_the_month_is_invalid()
    {
        this.ShouldResolveCurrentUser(Guid.Empty);

        Result result = await this._useCase.ExecuteAsync(CalendarEventBirthdayCreateCommandMother.Valid(month: 13));

        result.Error.Should().Be(CalendarEventErrors.AuthorRequired);
    }

    [Test]
    public async Task return_title_required_when_the_title_is_blank_and_the_month_is_invalid()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventBirthdayCreateCommandMother.Valid(title: "  ", month: 13));

        result.Error.Should().Be(CalendarEventErrors.TitleRequired);
    }

    [Test]
    public async Task return_user_not_found_without_asking_for_the_student_when_the_author_does_not_exist()
    {
        this.ShouldCaptureAddedCalendarEvents();
        Guid authorId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldNotFindUser(authorId);

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventBirthdayCreateCommandMother.Valid(studentId: studentId));

        result.Error.Should().Be(UserErrors.NotFound);
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<StudentFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_student_not_found_without_adding_when_the_student_does_not_exist()
    {
        this.ShouldCaptureAddedCalendarEvents();
        Guid studentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFailEnsuringStudentGuardian(studentId, authorId, new StudentExceptions.NotFound());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            CalendarEventBirthdayCreateCommandMother.Valid(studentId: studentId));

        await act.Should().ThrowAsync<StudentExceptions.NotFound>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_guardian_access_forbidden_without_adding_when_the_user_is_not_a_guardian_of_the_student()
    {
        this.ShouldCaptureAddedCalendarEvents();
        Guid studentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFailEnsuringStudentGuardian(studentId, authorId, new StudentExceptions.GuardianAccessForbidden());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            CalendarEventBirthdayCreateCommandMother.Valid(studentId: studentId));

        await act.Should().ThrowAsync<StudentExceptions.GuardianAccessForbidden>();
        this.Added.Should().BeEmpty();
    }
}

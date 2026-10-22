namespace SumiClas.Tests.ClassExpenses.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.CalendarEvents.Application;
using SumiClas.CalendarEvents.Domain;
using SumiClas.ClassExpenses.Application;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.CalendarEvents.Application;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class ClassExpenseCreateShould : ClassExpensesUnitTestCase
{
    private readonly ClassExpenseCreate.Create _useCase;

    public ClassExpenseCreateShould()
    {
        this._useCase = new ClassExpenseCreate.Create(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_the_expense_trimmed_with_unpaid_participants_the_author_and_the_clock_time()
    {
        Guid expenseId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid firstGuardianId = GuidMother.Random();
        Guid secondGuardianId = GuidMother.Random();
        string item = StringMother.Random("item");
        decimal totalAmount = 37.45m;
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindAllRequestedUsers();
        this.ShouldBeClassMember(firstGuardianId, classId);
        this.ShouldBeClassMember(secondGuardianId, classId);
        this.ShouldCaptureAddedClassExpenses();

        await this._useCase.ExecuteAsync(ClassExpenseCreateCommandMother.With(
            expenseId,
            classId,
            "  " + item + " ",
            totalAmount,
            [firstGuardianId, secondGuardianId],
            null));

        ClassExpense added = this.Added.Single();
        added.Id.Should().Be(expenseId);
        added.ClassId.Should().Be(classId);
        added.Item.Should().Be(item);
        added.TotalAmount.Should().Be(totalAmount);
        added.RelatedEventId.Should().BeNull();
        added.AuthorId.Should().Be(authorId);
        added.CreatedAt.Should().Be(Now);
        added.Participants.Should().BeEquivalentTo(
            [new ExpenseParticipant(firstGuardianId, false), new ExpenseParticipant(secondGuardianId, false)]);
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<CalendarEventFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task keep_the_related_event_when_it_exists()
    {
        Guid classId = GuidMother.Random();
        Guid eventId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        Guid guardianId = GuidMother.Random();
        this.ShouldFindAllRequestedUsers();
        this.ShouldBeClassMember(guardianId, classId);
        this.ShouldCaptureAddedClassExpenses();
        this.ShouldFindCalendarEvent(eventId);

        await this._useCase.ExecuteAsync(
            ClassExpenseCreateCommandMother.Valid(
                classId: classId,
                participantIds: [guardianId],
                relatedEventId: eventId));

        this.Added.Single().RelatedEventId.Should().Be(eventId);
        this.ShouldHaveAskedForCalendarEvent(eventId);
    }

    [Test]
    public async Task add_each_participant_once_and_leave_out_the_empty_ids()
    {
        Guid classId = GuidMother.Random();
        Guid firstGuardianId = GuidMother.Random();
        Guid secondGuardianId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindAllRequestedUsers();
        this.ShouldBeClassMember(firstGuardianId, classId);
        this.ShouldBeClassMember(secondGuardianId, classId);
        this.ShouldCaptureAddedClassExpenses();

        await this._useCase.ExecuteAsync(ClassExpenseCreateCommandMother.Valid(
            classId: classId,
            participantIds: [firstGuardianId, Guid.Empty, secondGuardianId, firstGuardianId]));

        this.Added.Single().Participants.Select(participant => participant.GuardianId)
            .Should().Equal(firstGuardianId, secondGuardianId);
        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<UserFinderByIds.Query>(query => query.UserIds.Count == 2
                    && query.UserIds.Contains(firstGuardianId)
                    && query.UserIds.Contains(secondGuardianId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task add_the_expense_when_the_amount_is_the_smallest_positive_one()
    {
        Guid classId = GuidMother.Random();
        decimal smallest = 0.01m;
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        Guid guardianId = GuidMother.Random();
        this.ShouldFindAllRequestedUsers();
        this.ShouldBeClassMember(guardianId, classId);
        this.ShouldCaptureAddedClassExpenses();

        await this._useCase.ExecuteAsync(
            ClassExpenseCreateCommandMother.Valid(
                classId: classId,
                participantIds: [guardianId],
                totalAmount: smallest));

        this.Added.Single().TotalAmount.Should().Be(smallest);
    }

    [Test]
    public async Task throw_class_representative_forbidden_without_adding_when_the_author_is_not_a_delegate()
    {
        this.ShouldCaptureAddedClassExpenses();
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldDenyClassRepresentative(authorId, classId);

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassExpenseCreateCommandMother.Valid(classId: classId));

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeForbidden>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_participant_not_in_class_without_adding_when_a_participant_does_not_belong_to_the_class()
    {
        this.ShouldCaptureAddedClassExpenses();
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid memberId = GuidMother.Random();
        Guid outsiderId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindAllRequestedUsers();
        this.ShouldBeClassMember(memberId, classId);
        this.ShouldNotBeClassMember(outsiderId, classId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseCreateCommandMother.Valid(classId: classId, participantIds: [memberId, outsiderId]));

        await act.Should().ThrowAsync<ClassExpenseExceptions.ParticipantNotInClass>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_unauthenticated_without_asking_or_adding_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();
        this.ShouldCaptureAddedClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassExpenseCreateCommandMother.Valid());

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_amount_must_be_positive_without_asking_or_adding_when_the_amount_is_negative()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassExpenseCreateCommandMother.Valid(totalAmount: -1));

        await act.Should().ThrowAsync<ClassExpenseExceptions.TotalAmountMustBePositive>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_participant_required_without_asking_or_adding_when_there_are_no_participants()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassExpenseCreateCommandMother.Valid(participantIds: []));

        await act.Should().ThrowAsync<ClassExpenseExceptions.AtLeastOneParticipantRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_participant_required_when_the_only_participant_ids_are_empty()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseCreateCommandMother.Valid(participantIds: [Guid.Empty, Guid.Empty]));

        await act.Should().ThrowAsync<ClassExpenseExceptions.AtLeastOneParticipantRequired>();
    }

    [Test]
    public async Task throw_id_required_without_asking_or_adding_when_the_id_and_the_class_are_empty()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseCreateCommandMother.Valid(id: Guid.Empty, classId: Guid.Empty));

        await act.Should().ThrowAsync<ClassExpenseExceptions.IdRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_class_required_without_asking_or_adding_when_the_class_is_empty_and_the_item_is_blank()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseCreateCommandMother.Valid(classId: Guid.Empty, item: "  "));

        await act.Should().ThrowAsync<ClassExpenseExceptions.ClassRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_item_required_without_asking_or_adding_when_the_item_is_blank_and_the_amount_is_zero()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseCreateCommandMother.Valid(item: "  ", totalAmount: 0));

        await act.Should().ThrowAsync<ClassExpenseExceptions.ItemRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_amount_must_be_positive_without_asking_or_adding_when_it_is_zero_and_the_user_id_is_empty()
    {
        this.ShouldResolveCurrentUser(Guid.Empty);
        this.ShouldCaptureAddedClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassExpenseCreateCommandMother.Valid(totalAmount: 0));

        await act.Should().ThrowAsync<ClassExpenseExceptions.TotalAmountMustBePositive>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_author_required_without_asking_or_adding_when_the_user_id_is_empty_and_no_participants()
    {
        this.ShouldResolveCurrentUser(Guid.Empty);
        this.ShouldCaptureAddedClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassExpenseCreateCommandMother.Valid(participantIds: []));

        await act.Should().ThrowAsync<ClassExpenseExceptions.AuthorRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_user_not_found_without_asking_for_the_class_when_the_author_does_not_exist()
    {
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldNotFindUser(authorId);
        this.ShouldCaptureAddedClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassExpenseCreateCommandMother.Valid());

        await act.Should().ThrowAsync<UserExceptions.NotFound>();
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<ClassFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_class_not_found_without_asking_for_the_school_year_when_the_class_does_not_exist()
    {
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldNotFindClass();
        this.ShouldCaptureAddedClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassExpenseCreateCommandMother.Valid());

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<SchoolYearEnsureIsActive.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_school_year_archived_without_asking_for_the_participants_when_it_is_archived()
    {
        Guid authorId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());
        this.ShouldCaptureAddedClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassExpenseCreateCommandMother.Valid(classId: classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<UserFinderByIds.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_school_year_not_found_without_adding_when_the_school_year_does_not_exist()
    {
        Guid authorId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.NotFound());
        this.ShouldCaptureAddedClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassExpenseCreateCommandMother.Valid(classId: classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_participant_not_found_without_asking_for_the_event_when_a_participant_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        Guid eventId = GuidMother.Random();
        Guid existingGuardianId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindOnlyUsers(existingGuardianId);
        this.ShouldCaptureAddedClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassExpenseCreateCommandMother.Valid(
            classId: classId,
            participantIds: [existingGuardianId, GuidMother.Random()],
            relatedEventId: eventId));

        await act.Should().ThrowAsync<ClassExpenseExceptions.ParticipantNotFound>();
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<CalendarEventFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_calendar_event_not_found_without_adding_when_the_related_event_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        Guid eventId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        Guid guardianId = GuidMother.Random();
        this.ShouldFindAllRequestedUsers();
        this.ShouldBeClassMember(guardianId, classId);
        this.ShouldCaptureAddedClassExpenses();
        this.ShouldNotFindCalendarEvent(eventId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseCreateCommandMother.Valid(
                classId: classId,
                participantIds: [guardianId],
                relatedEventId: eventId));

        await act.Should().ThrowAsync<CalendarEventExceptions.NotFound>();
        this.Added.Should().BeEmpty();
    }

    private void ShouldFindUser(Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserResponseMother.With(userId, StringMother.Random("name")));

    private void ShouldNotFindUser(Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UserExceptions.NotFound());

    private void ShouldFindAllRequestedUsers()
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<UserFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserFinderByIds.Query query, CancellationToken _)
                => (IReadOnlyCollection<UserResponse>)query.UserIds
                    .Select(userId => UserResponseMother.With(userId, StringMother.Random("name")))
                    .ToList());

    private void ShouldFindOnlyUsers(params Guid[] userIds)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<UserFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserFinderByIds.Query query, CancellationToken _)
                => (IReadOnlyCollection<UserResponse>)query.UserIds
                    .Where(userId => userIds.Contains(userId))
                    .Select(userId => UserResponseMother.With(userId, StringMother.Random("name")))
                    .ToList());

    private void ShouldFindCalendarEvent(Guid calendarEventId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<CalendarEventFinderById.Query>(query => query.CalendarEventId == calendarEventId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                CalendarEventSummaryResponseMother.With(calendarEventId, StringMother.Random("Event")));

    private void ShouldHaveAskedForCalendarEvent(Guid calendarEventId)
        => this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<CalendarEventFinderById.Query>(query => query.CalendarEventId == calendarEventId),
                It.IsAny<CancellationToken>()),
            Times.Once);

    private void ShouldNotFindCalendarEvent(Guid calendarEventId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<CalendarEventFinderById.Query>(query => query.CalendarEventId == calendarEventId),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CalendarEventExceptions.NotFound());
}

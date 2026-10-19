namespace SumiClas.Tests.ClassExpenses.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.ClassExpenses.Application;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.ClassExpenses.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class ClassExpensePaidMarkShould : ClassExpensesUnitTestCase
{
    private readonly ClassExpensePaidMark.Mark _useCase;

    public ClassExpensePaidMarkShould()
    {
        this._useCase = new ClassExpensePaidMark.Mark(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task mark_the_guardian_as_paid_and_leave_the_other_participants_and_expenses_unchanged()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        Guid otherUnpaidId = GuidMother.Random();
        Guid otherPaidId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants:
            [
                ExpenseParticipantMother.Unpaid(otherUnpaidId),
                ExpenseParticipantMother.Unpaid(guardianId),
                ExpenseParticipantMother.Paid(otherPaidId),
            ]);
        ClassExpense other = ClassExpenseMother.With(participants: [ExpenseParticipantMother.Unpaid(guardianId)]);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, expense.ClassId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchClassExpenses(other, expense);

        Result result = await this._useCase.ExecuteAsync(
            ClassExpensePaidMarkCommandMother.With(expenseId, guardianId));

        result.IsSuccess.Should().BeTrue();
        other.Participants.Should().BeEquivalentTo([ExpenseParticipantMother.Unpaid(guardianId)]);
        expense.Participants.Should().BeEquivalentTo(
        [
            ExpenseParticipantMother.Unpaid(otherUnpaidId),
            ExpenseParticipantMother.Paid(guardianId),
            ExpenseParticipantMother.Paid(otherPaidId),
        ]);
    }

    [Test]
    public async Task keep_the_guardian_as_paid_when_it_was_already_paid()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants: [ExpenseParticipantMother.Paid(guardianId)]);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, expense.ClassId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchClassExpenses(expense);

        Result result = await this._useCase.ExecuteAsync(
            ClassExpensePaidMarkCommandMother.With(expenseId, guardianId));

        result.IsSuccess.Should().BeTrue();
        expense.Participants.Should().BeEquivalentTo([ExpenseParticipantMother.Paid(guardianId)]);
    }

    [Test]
    public async Task not_add_any_expense()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants: [ExpenseParticipantMother.Unpaid(guardianId)]);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, expense.ClassId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchClassExpenses(expense);
        this.ShouldCaptureAddedClassExpenses();

        await this._useCase.ExecuteAsync(ClassExpensePaidMarkCommandMother.With(expenseId, guardianId));

        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_not_found_without_asking_when_the_expense_does_not_exist()
    {
        this.ShouldFetchNoClassExpenses();

        Result result = await this._useCase.ExecuteAsync(
            ClassExpensePaidMarkCommandMother.With(GuidMother.Random(), GuidMother.Random()));

        result.Error.Should().Be(ClassExpenseErrors.NotFound);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_not_found_without_asking_for_the_school_year_when_the_class_does_not_exist()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants: [ExpenseParticipantMother.Unpaid(guardianId)]);
        this.ShouldNotFindClass();
        this.ShouldFetchClassExpenses(expense);

        Result result = await this._useCase.ExecuteAsync(
            ClassExpensePaidMarkCommandMother.With(expenseId, guardianId));

        result.Error.Should().Be(ClassErrors.NotFound);
        expense.Participants.Single().HasPaid.Should().BeFalse();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<SchoolYearEnsureIsActive.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task return_school_year_archived_without_marking_when_the_school_year_is_archived()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants: [ExpenseParticipantMother.Unpaid(guardianId)]);
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, expense.ClassId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);
        this.ShouldFetchClassExpenses(expense);

        Result result = await this._useCase.ExecuteAsync(
            ClassExpensePaidMarkCommandMother.With(expenseId, guardianId));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        expense.Participants.Single().HasPaid.Should().BeFalse();
    }

    [Test]
    public async Task return_school_year_archived_when_the_guardian_is_not_a_participant_and_it_is_archived()
    {
        Guid expenseId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(id: expenseId);
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, expense.ClassId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);
        this.ShouldFetchClassExpenses(expense);

        Result result = await this._useCase.ExecuteAsync(
            ClassExpensePaidMarkCommandMother.With(expenseId, GuidMother.Random()));

        result.Error.Should().Be(SchoolYearErrors.Archived);
    }

    [Test]
    public async Task return_not_a_participant_and_leave_the_participants_unchanged_for_an_unknown_guardian()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants: [ExpenseParticipantMother.Unpaid(guardianId)]);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, expense.ClassId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchClassExpenses(expense);

        Result result = await this._useCase.ExecuteAsync(
            ClassExpensePaidMarkCommandMother.With(expenseId, GuidMother.Random()));

        result.Error.Should().Be(ClassExpenseErrors.NotAParticipant);
        expense.Participants.Should().BeEquivalentTo([ExpenseParticipantMother.Unpaid(guardianId)]);
    }

    [Test]
    public async Task return_class_representative_forbidden_without_marking_when_the_user_is_not_a_delegate()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants: [ExpenseParticipantMother.Unpaid(guardianId)]);
        Guid schoolYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldDenyClassRepresentative(currentUserId, expense.ClassId);
        this.ShouldFetchClassExpenses(expense);

        Result result = await this._useCase.ExecuteAsync(ClassExpensePaidMarkCommandMother.With(expenseId, guardianId));

        result.Error.Should().Be(UserErrors.ClassRepresentativeForbidden);
        expense.Participants.Should().BeEquivalentTo([ExpenseParticipantMother.Unpaid(guardianId)]);
    }

    [Test]
    public async Task return_unauthenticated_without_marking_when_there_is_no_current_user()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants: [ExpenseParticipantMother.Unpaid(guardianId)]);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        this.ShouldHaveNoCurrentUser();
        this.ShouldFetchClassExpenses(expense);

        Result result = await this._useCase.ExecuteAsync(ClassExpensePaidMarkCommandMother.With(expenseId, guardianId));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        expense.Participants.Should().BeEquivalentTo([ExpenseParticipantMother.Unpaid(guardianId)]);
    }
}

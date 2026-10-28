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
public class ClassExpenseUnpaidMarkShould : ClassExpensesUnitTestCase
{
    private readonly ClassExpenseUnpaidMark.Mark _useCase;

    public ClassExpenseUnpaidMarkShould()
    {
        this._useCase = new ClassExpenseUnpaidMark.Mark(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task mark_the_guardian_as_unpaid_and_leave_the_other_participants_and_expenses_unchanged()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        Guid otherUnpaidId = GuidMother.Random();
        Guid otherPaidId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants:
            [
                ExpenseParticipantMother.Paid(otherPaidId),
                ExpenseParticipantMother.Paid(guardianId),
                ExpenseParticipantMother.Unpaid(otherUnpaidId),
            ]);
        ClassExpense other = ClassExpenseMother.With(participants: [ExpenseParticipantMother.Paid(guardianId)]);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, expense.ClassId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchClassExpenses(other, expense);

        await this._useCase.ExecuteAsync(
            ClassExpenseUnpaidMarkCommandMother.With(expenseId, guardianId));

        other.Participants.Should().BeEquivalentTo([ExpenseParticipantMother.Paid(guardianId)]);
        expense.Participants.Should().BeEquivalentTo(
        [
            ExpenseParticipantMother.Paid(otherPaidId),
            ExpenseParticipantMother.Unpaid(guardianId),
            ExpenseParticipantMother.Unpaid(otherUnpaidId),
        ]);
    }

    [Test]
    public async Task keep_the_guardian_as_unpaid_when_it_was_already_unpaid()
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

        await this._useCase.ExecuteAsync(
            ClassExpenseUnpaidMarkCommandMother.With(expenseId, guardianId));

        expense.Participants.Should().BeEquivalentTo([ExpenseParticipantMother.Unpaid(guardianId)]);
    }

    [Test]
    public async Task not_add_any_expense()
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
        this.ShouldCaptureAddedClassExpenses();

        await this._useCase.ExecuteAsync(ClassExpenseUnpaidMarkCommandMother.With(expenseId, guardianId));

        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_not_found_without_asking_when_the_expense_does_not_exist()
    {
        this.ShouldFetchNoClassExpenses();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseUnpaidMarkCommandMother.With(GuidMother.Random(), GuidMother.Random()));

        await act.Should().ThrowAsync<ClassExpenseExceptions.NotFound>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_class_not_found_without_asking_for_the_school_year_when_the_class_does_not_exist()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants: [ExpenseParticipantMother.Paid(guardianId)]);
        this.ShouldNotFindClass();
        this.ShouldFetchClassExpenses(expense);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseUnpaidMarkCommandMother.With(expenseId, guardianId));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        expense.Participants.Single().HasPaid.Should().BeTrue();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<SchoolYearEnsureIsActive.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_school_year_archived_when_the_guardian_is_not_a_participant_and_it_is_archived()
    {
        Guid expenseId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(id: expenseId);
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, expense.ClassId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());
        this.ShouldFetchClassExpenses(expense);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseUnpaidMarkCommandMother.With(expenseId, GuidMother.Random()));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
    }

    [Test]
    public async Task throw_not_a_participant_and_leave_the_participants_unchanged_for_an_unknown_guardian()
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

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseUnpaidMarkCommandMother.With(expenseId, GuidMother.Random()));

        await act.Should().ThrowAsync<ClassExpenseExceptions.NotAParticipant>();
        expense.Participants.Should().BeEquivalentTo([ExpenseParticipantMother.Paid(guardianId)]);
    }

    [Test]
    public async Task throw_class_representative_forbidden_without_marking_when_the_user_is_not_a_delegate()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants: [ExpenseParticipantMother.Paid(guardianId)]);
        Guid schoolYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldDenyClassRepresentative(currentUserId, expense.ClassId);
        this.ShouldFetchClassExpenses(expense);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseUnpaidMarkCommandMother.With(expenseId, guardianId));

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeForbidden>();
        expense.Participants.Should().BeEquivalentTo([ExpenseParticipantMother.Paid(guardianId)]);
    }

    [Test]
    public async Task throw_unauthenticated_without_marking_when_there_is_no_current_user()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants: [ExpenseParticipantMother.Paid(guardianId)]);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        this.ShouldHaveNoCurrentUser();
        this.ShouldFetchClassExpenses(expense);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseUnpaidMarkCommandMother.With(expenseId, guardianId));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        expense.Participants.Should().BeEquivalentTo([ExpenseParticipantMother.Paid(guardianId)]);
    }

    [Test]
    public async Task mark_the_own_share_as_unpaid_when_the_user_is_in_the_class_without_being_a_delegate()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants: [ExpenseParticipantMother.Paid(guardianId)]);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldAllowClassAccess(guardianId, expense.ClassId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchClassExpenses(expense);

        await this._useCase.ExecuteAsync(ClassExpenseUnpaidMarkCommandMother.With(expenseId, guardianId));

        expense.Participants.Should().BeEquivalentTo([ExpenseParticipantMother.Unpaid(guardianId)]);
    }

    [Test]
    public async Task throw_class_access_forbidden_when_the_own_share_is_marked_without_being_in_the_class()
    {
        Guid expenseId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        ClassExpense expense = ClassExpenseMother.With(
            id: expenseId,
            participants: [ExpenseParticipantMother.Paid(guardianId)]);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(expense.ClassId, schoolYearId);
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldDenyClassAccess(guardianId, expense.ClassId);
        this.ShouldFetchClassExpenses(expense);

        Func<Task> act = () => this._useCase.ExecuteAsync(

            ClassExpenseUnpaidMarkCommandMother.With(expenseId, guardianId));

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        expense.Participants.Should().BeEquivalentTo([ExpenseParticipantMother.Paid(guardianId)]);
    }
}

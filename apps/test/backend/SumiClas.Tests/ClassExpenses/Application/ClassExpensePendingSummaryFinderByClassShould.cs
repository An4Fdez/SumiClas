namespace SumiClas.Tests.ClassExpenses.Application;

using AwesomeAssertions;
using SumiClas.ClassExpenses.Application;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Classes.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.ClassExpenses.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class ClassExpensePendingSummaryFinderByClassShould : ClassExpensesUnitTestCase
{
    private const int ONE_MINUTE = 1;
    private const int TWO_MINUTES = 2;

    private readonly ClassExpensePendingSummaryFinderByClass.Finder _useCase;

    public ClassExpensePendingSummaryFinderByClassShould()
    {
        this._useCase = new ClassExpensePendingSummaryFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task return_the_item_of_the_expense_the_current_user_has_not_paid()
    {
        Guid currentUserId = GuidMother.Random();
        Guid classId = this.ShouldResolveClassOfCurrentUser(currentUserId);
        ClassExpense owed = ClassExpenseMother.With(
            classId: classId,
            participants: [ExpenseParticipantMother.Unpaid(currentUserId)]);
        this.ShouldSearchClassExpenses(owed);

        ClassExpensePendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Should().Be(new ClassExpensePendingSummaryResponse(Pending: true, Item: owed.Item));
    }

    [Test]
    public async Task return_the_oldest_expense_when_the_current_user_owes_several()
    {
        Guid currentUserId = GuidMother.Random();
        Guid classId = this.ShouldResolveClassOfCurrentUser(currentUserId);
        ClassExpense oldest = ClassExpenseMother.With(
            classId: classId,
            createdAt: Now.AddMinutes(ONE_MINUTE),
            participants: [ExpenseParticipantMother.Unpaid(currentUserId)]);
        this.ShouldSearchClassExpenses(
            ClassExpenseMother.With(
                classId: classId,
                createdAt: Now.AddMinutes(TWO_MINUTES),
                participants: [ExpenseParticipantMother.Unpaid(currentUserId)]),
            oldest);

        ClassExpensePendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Item.Should().Be(oldest.Item);
    }

    [Test]
    public async Task pick_the_expense_with_the_lowest_id_when_the_oldest_ones_were_created_at_the_same_time()
    {
        Guid currentUserId = GuidMother.Random();
        Guid classId = this.ShouldResolveClassOfCurrentUser(currentUserId);
        ClassExpense low = ClassExpenseMother.With(
            id: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            classId: classId,
            participants: [ExpenseParticipantMother.Unpaid(currentUserId)]);
        ClassExpense high = ClassExpenseMother.With(
            id: Guid.Parse("ff000000-0000-0000-0000-000000000000"),
            classId: classId,
            participants: [ExpenseParticipantMother.Unpaid(currentUserId)]);
        this.ShouldSearchClassExpenses(high, low);

        ClassExpensePendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Item.Should().Be(low.Item);
    }

    [Test]
    public async Task not_count_the_expenses_the_current_user_has_already_paid()
    {
        Guid currentUserId = GuidMother.Random();
        Guid classId = this.ShouldResolveClassOfCurrentUser(currentUserId);
        this.ShouldSearchClassExpenses(ClassExpenseMother.With(
            classId: classId,
            participants: [ExpenseParticipantMother.Paid(currentUserId)]));

        ClassExpensePendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Should().Be(new ClassExpensePendingSummaryResponse(Pending: false, Item: null));
    }

    [Test]
    public async Task not_count_the_expenses_another_participant_has_not_paid()
    {
        Guid currentUserId = GuidMother.Random();
        Guid classId = this.ShouldResolveClassOfCurrentUser(currentUserId);
        this.ShouldSearchClassExpenses(ClassExpenseMother.With(
            classId: classId,
            participants:
            [
                ExpenseParticipantMother.Paid(currentUserId),
                ExpenseParticipantMother.Unpaid(GuidMother.Random()),
            ]));

        ClassExpensePendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Pending.Should().BeFalse();
    }

    [Test]
    public async Task not_count_the_expenses_the_current_user_does_not_take_part_in()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        this.ShouldSearchClassExpenses(ClassExpenseMother.With(classId: classId));

        ClassExpensePendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Pending.Should().BeFalse();
    }

    [Test]
    public async Task not_count_the_expenses_of_other_classes()
    {
        Guid currentUserId = GuidMother.Random();
        Guid classId = this.ShouldResolveClassOfCurrentUser(currentUserId);
        this.ShouldSearchClassExpenses(ClassExpenseMother.With(
            classId: GuidMother.Random(),
            participants: [ExpenseParticipantMother.Unpaid(currentUserId)]));

        ClassExpensePendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Pending.Should().BeFalse();
    }

    [Test]
    public async Task throw_unauthenticated_without_asking_or_reading_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this.SummarizeAsync(GuidMother.Random());

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.QueryBus.VerifyNoOtherCalls();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_class_access_forbidden_without_reading_when_the_user_does_not_belong_to_the_class()
    {
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldDenyClassAccess(currentUserId, classId);

        Func<Task> act = () => this.SummarizeAsync(classId);

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_the_class_error_when_the_class_is_unknown()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldNotFindClass();

        Func<Task> act = () => this.SummarizeAsync(GuidMother.Random());

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.ShouldNotHaveSearched();
    }

    private Task<ClassExpensePendingSummaryResponse> SummarizeAsync(Guid classId)
        => this._useCase.ExecuteAsync(
            ClassExpensePendingSummaryFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

    private Guid ShouldResolveClassOfCurrentUser(Guid? userId = null)
    {
        Guid classId = GuidMother.Random();
        Guid currentUserId = userId ?? GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        return classId;
    }
}

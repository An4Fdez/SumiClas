namespace SumiClas.Tests.ClassExpenses.Application;

using AwesomeAssertions;
using SumiClas.ClassExpenses.Application;
using SumiClas.Classes.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.ClassExpenses.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class ClassExpenseOpenFinderByClassShould : ClassExpensesUnitTestCase
{
    private const int ONE_MINUTE = 1;
    private const int TWO_MINUTES = 2;

    private readonly ClassExpenseOpenFinderByClass.Finder _useCase;

    public ClassExpenseOpenFinderByClassShould()
    {
        this._useCase = new ClassExpenseOpenFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task return_what_the_expenses_with_someone_who_has_not_paid_are_for_oldest_first()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        string oldest = StringMother.Random("oldest");
        string newest = StringMother.Random("newest");
        this.ShouldSearchClassExpenses(
            ClassExpenseMother.With(
                classId: classId,
                item: newest,
                createdAt: Now.AddMinutes(TWO_MINUTES),
                participants: [ExpenseParticipantMother.Unpaid(GuidMother.Random())]),
            ClassExpenseMother.With(
                classId: classId,
                item: oldest,
                createdAt: Now.AddMinutes(ONE_MINUTE),
                participants: [ExpenseParticipantMother.Unpaid(GuidMother.Random())]));

        IReadOnlyCollection<ClassExpenseOpenResponse> result = await this.FindOpenAsync(classId);

        result.Should().Equal(new ClassExpenseOpenResponse(oldest), new ClassExpenseOpenResponse(newest));
    }

    [Test]
    public async Task keep_listing_an_expense_while_only_some_of_its_participants_have_paid()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        string partlyPaid = StringMother.Random("partly paid");
        this.ShouldSearchClassExpenses(ClassExpenseMother.With(
            classId: classId,
            item: partlyPaid,
            participants:
            [
                ExpenseParticipantMother.Paid(GuidMother.Random()),
                ExpenseParticipantMother.Unpaid(GuidMother.Random()),
            ]));

        IReadOnlyCollection<ClassExpenseOpenResponse> result = await this.FindOpenAsync(classId);

        result.Should().Equal(new ClassExpenseOpenResponse(partlyPaid));
    }

    [Test]
    public async Task not_list_the_expenses_every_participant_has_paid()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        string open = StringMother.Random("open");
        this.ShouldSearchClassExpenses(
            ClassExpenseMother.With(
                classId: classId,
                item: open,
                participants: [ExpenseParticipantMother.Unpaid(GuidMother.Random())]),
            ClassExpenseMother.With(
                classId: classId,
                participants:
                [
                    ExpenseParticipantMother.Paid(GuidMother.Random()),
                    ExpenseParticipantMother.Paid(GuidMother.Random()),
                ]));

        IReadOnlyCollection<ClassExpenseOpenResponse> result = await this.FindOpenAsync(classId);

        result.Select(expense => expense.Item).Should().Equal(open);
    }

    [Test]
    public async Task order_the_expenses_created_at_the_same_instant_by_id()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        string low = StringMother.Random("low");
        string high = StringMother.Random("high");
        this.ShouldSearchClassExpenses(
            ClassExpenseMother.With(
                id: Guid.Parse("ff000000-0000-0000-0000-000000000000"),
                classId: classId,
                item: high),
            ClassExpenseMother.With(
                id: Guid.Parse("00000000-0000-0000-0000-000000000001"),
                classId: classId,
                item: low));

        IReadOnlyCollection<ClassExpenseOpenResponse> result = await this.FindOpenAsync(classId);

        result.Select(expense => expense.Item).Should().Equal(low, high);
    }

    [Test]
    public async Task give_the_same_answer_to_whoever_asks_whether_they_owe_have_paid_or_take_no_part()
    {
        Guid currentUserId = GuidMother.Random();
        Guid classId = this.ShouldResolveClassOfCurrentUser(currentUserId);
        string owed = StringMother.Random("owed");
        string paidByMe = StringMother.Random("paid by me");
        string notMine = StringMother.Random("not mine");
        this.ShouldSearchClassExpenses(
            ClassExpenseMother.With(
                classId: classId,
                item: owed,
                createdAt: Now.AddMinutes(ONE_MINUTE),
                participants: [ExpenseParticipantMother.Unpaid(currentUserId)]),
            ClassExpenseMother.With(
                classId: classId,
                item: paidByMe,
                createdAt: Now.AddMinutes(TWO_MINUTES),
                participants:
                [
                    ExpenseParticipantMother.Paid(currentUserId),
                    ExpenseParticipantMother.Unpaid(GuidMother.Random()),
                ]),
            ClassExpenseMother.With(
                classId: classId,
                item: notMine,
                createdAt: Now.AddMinutes(TWO_MINUTES + ONE_MINUTE),
                participants: [ExpenseParticipantMother.Unpaid(GuidMother.Random())]));

        IReadOnlyCollection<ClassExpenseOpenResponse> result = await this.FindOpenAsync(classId);

        result.Select(expense => expense.Item).Should().Equal(owed, paidByMe, notMine);
    }

    [Test]
    public async Task not_list_the_expenses_of_other_classes()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        string mine = StringMother.Random("mine");
        this.ShouldSearchClassExpenses(
            ClassExpenseMother.With(classId: classId, item: mine),
            ClassExpenseMother.With(classId: GuidMother.Random()));

        IReadOnlyCollection<ClassExpenseOpenResponse> result = await this.FindOpenAsync(classId);

        result.Select(expense => expense.Item).Should().Equal(mine);
    }

    [Test]
    public async Task return_an_empty_list_when_there_are_no_expenses()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        this.ShouldSearchNoClassExpenses();

        IReadOnlyCollection<ClassExpenseOpenResponse> result = await this.FindOpenAsync(classId);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task throw_unauthenticated_without_asking_or_reading_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this.FindOpenAsync(GuidMother.Random());

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

        Func<Task> act = () => this.FindOpenAsync(classId);

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_the_class_error_when_the_class_is_unknown()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldNotFindClass();

        Func<Task> act = () => this.FindOpenAsync(GuidMother.Random());

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.ShouldNotHaveSearched();
    }

    private Task<IReadOnlyCollection<ClassExpenseOpenResponse>> FindOpenAsync(Guid classId)
        => this._useCase.ExecuteAsync(
            ClassExpenseOpenFinderByClassQueryMother.WithClassId(classId),
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

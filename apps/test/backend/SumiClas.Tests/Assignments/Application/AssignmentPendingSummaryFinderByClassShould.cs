namespace SumiClas.Tests.Assignments.Application;

using AwesomeAssertions;
using SumiClas.Assignments.Application;
using SumiClas.Assignments.Domain;
using SumiClas.Classes.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Assignments.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class AssignmentPendingSummaryFinderByClassShould : AssignmentsUnitTestCase
{
    private const int ONE_DAY = 1;
    private const int A_WEEK = 7;
    private const int NONE = 0;

    private static readonly DateOnly TODAY = DateOnly.FromDateTime(Now.UtcDateTime);
    private static readonly DateOnly YESTERDAY = TODAY.AddDays(-ONE_DAY);
    private static readonly DateOnly TOMORROW = TODAY.AddDays(ONE_DAY);
    private static readonly DateOnly NEXT_WEEK = TODAY.AddDays(A_WEEK);

    private readonly AssignmentPendingSummaryFinderByClass.Finder _useCase;

    public AssignmentPendingSummaryFinderByClassShould()
    {
        this._useCase = new AssignmentPendingSummaryFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object,
            this.Clock.Object);
    }

    [Test]
    public async Task count_the_assignments_due_today_or_later()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        Assignment[] pending =
        [
            AssignmentMother.With(classId: classId, dueDate: TODAY),
            AssignmentMother.With(classId: classId, dueDate: TOMORROW),
            AssignmentMother.With(classId: classId, dueDate: NEXT_WEEK),
        ];
        this.ShouldSearchAssignments(pending);

        AssignmentPendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Pending.Should().Be(pending.Length);
    }

    [Test]
    public async Task not_count_the_assignments_already_past_due()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        Assignment[] pending = [AssignmentMother.With(classId: classId, dueDate: TODAY)];
        this.ShouldSearchAssignments([AssignmentMother.With(classId: classId, dueDate: YESTERDAY), .. pending]);

        AssignmentPendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Pending.Should().Be(pending.Length);
    }

    [Test]
    public async Task not_count_the_assignments_the_current_user_marked_as_done_but_count_the_ones_others_marked()
    {
        Guid currentUserId = GuidMother.Random();
        Guid classId = this.ShouldResolveClassOfCurrentUser(currentUserId);
        Assignment[] pending =
        [
            AssignmentMother.DoneBy(GuidMother.Random(), GuidMother.Random(), classId, TOMORROW),
            AssignmentMother.With(classId: classId, dueDate: TOMORROW),
        ];
        this.ShouldSearchAssignments(
        [
            AssignmentMother.DoneBy(GuidMother.Random(), currentUserId, classId, TOMORROW),
            .. pending,
        ]);

        AssignmentPendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Pending.Should().Be(pending.Length);
    }

    [Test]
    public async Task return_the_earliest_due_date_among_the_pending_ones_and_not_among_the_done_ones()
    {
        Guid currentUserId = GuidMother.Random();
        Guid classId = this.ShouldResolveClassOfCurrentUser(currentUserId);
        this.ShouldSearchAssignments(
            AssignmentMother.With(classId: classId, dueDate: NEXT_WEEK),
            AssignmentMother.DoneBy(GuidMother.Random(), currentUserId, classId, TODAY),
            AssignmentMother.With(classId: classId, dueDate: TOMORROW),
            AssignmentMother.With(classId: classId, dueDate: YESTERDAY));

        AssignmentPendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.NextDueDate.Should().Be(TOMORROW);
    }

    [Test]
    public async Task return_no_pending_and_no_due_date_when_there_is_nothing_to_do()
    {
        Guid currentUserId = GuidMother.Random();
        Guid classId = this.ShouldResolveClassOfCurrentUser(currentUserId);
        this.ShouldSearchAssignments(
            AssignmentMother.DoneBy(GuidMother.Random(), currentUserId, classId, TOMORROW),
            AssignmentMother.With(classId: classId, dueDate: YESTERDAY));

        AssignmentPendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Should().Be(new AssignmentPendingSummaryResponse(Pending: NONE, NextDueDate: null));
    }

    [Test]
    public async Task count_the_proposed_assignments_as_well_as_the_official_ones()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        Assignment[] pending =
        [
            AssignmentMother.With(classId: classId, dueDate: TOMORROW, isProposal: true),
            AssignmentMother.With(classId: classId, dueDate: TOMORROW, isProposal: false),
        ];
        this.ShouldSearchAssignments(pending);

        AssignmentPendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Pending.Should().Be(pending.Length);
    }

    [Test]
    public async Task not_count_the_assignments_of_other_classes()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        Assignment[] mine = [AssignmentMother.With(classId: classId, dueDate: TOMORROW)];
        this.ShouldSearchAssignments([.. mine, AssignmentMother.With(classId: GuidMother.Random(), dueDate: TOMORROW)]);

        AssignmentPendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Pending.Should().Be(mine.Length);
    }

    [Test]
    public async Task take_today_from_the_utc_date_of_the_clock()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        this.ShouldSearchAssignments(AssignmentMother.With(classId: classId, dueDate: TODAY));
        this.AdvanceClock(TimeSpan.FromDays(ONE_DAY));

        AssignmentPendingSummaryResponse result = await this.SummarizeAsync(classId);

        result.Should().Be(new AssignmentPendingSummaryResponse(Pending: NONE, NextDueDate: null));
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

    private Task<AssignmentPendingSummaryResponse> SummarizeAsync(Guid classId)
        => this._useCase.ExecuteAsync(
            AssignmentPendingSummaryFinderByClassQueryMother.WithClassId(classId),
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

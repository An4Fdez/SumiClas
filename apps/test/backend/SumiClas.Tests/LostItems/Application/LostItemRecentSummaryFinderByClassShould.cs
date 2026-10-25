namespace SumiClas.Tests.LostItems.Application;

using AwesomeAssertions;
using SumiClas.Classes.Domain;
using SumiClas.LostItems.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.LostItems.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class LostItemRecentSummaryFinderByClassShould : LostItemsUnitTestCase
{
    private const int REQUESTED_DAYS = 7;
    private const int ONE_DAY = 1;
    private const int TWO_DAYS = 2;
    private const int ONE_SECOND = 1;

    private static readonly TimeSpan YESTERDAY = TimeSpan.FromDays(ONE_DAY);
    private static readonly TimeSpan TWO_DAYS_AGO = TimeSpan.FromDays(TWO_DAYS);
    private static readonly TimeSpan JUST_OUTSIDE = TimeSpan.FromSeconds(ONE_SECOND);

    private readonly LostItemRecentSummaryFinderByClass.Finder _useCase;

    public LostItemRecentSummaryFinderByClassShould()
    {
        this._useCase = new LostItemRecentSummaryFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object,
            this.Clock.Object);
    }

    [Test]
    public async Task count_the_unresolved_items_of_the_school_and_of_the_class()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        LostItem[] unresolved =
        [
            LostItemMother.With(schoolId: schoolId, createdAt: Now - YESTERDAY),
            LostItemMother.With(schoolId: schoolId, status: ItemStatus.Found, createdAt: Now - TWO_DAYS_AGO),
            LostItemMother.ForClass(classId, createdAt: Now),
        ];
        this.ShouldSearchLostItems(unresolved);

        LostItemRecentSummaryResponse result = await this.SummarizeAsync(classId);

        result.Count.Should().Be(unresolved.Length);
    }

    [Test]
    public async Task not_count_the_items_already_recovered()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        LostItem[] unresolved = [LostItemMother.With(schoolId: schoolId, createdAt: Now)];
        this.ShouldSearchLostItems(
        [
            .. unresolved,
            LostItemMother.With(schoolId: schoolId, recovered: true, createdAt: Now),
            LostItemMother.ForClass(classId, recovered: true, createdAt: Now),
        ]);

        LostItemRecentSummaryResponse result = await this.SummarizeAsync(classId);

        result.Count.Should().Be(unresolved.Length);
    }

    [Test]
    public async Task not_count_the_items_the_current_user_posted_but_count_the_ones_of_others()
    {
        Guid currentUserId = GuidMother.Random();
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser(currentUserId);
        LostItem[] ofOthers =
        [
            LostItemMother.With(schoolId: schoolId, authorId: GuidMother.Random(), createdAt: Now),
            LostItemMother.With(
                scope: ItemScope.Class,
                classId: classId,
                authorId: GuidMother.Random(),
                createdAt: Now),
        ];
        this.ShouldSearchLostItems(
        [
            LostItemMother.With(schoolId: schoolId, authorId: currentUserId, createdAt: Now),
            LostItemMother.With(scope: ItemScope.Class, classId: classId, authorId: currentUserId, createdAt: Now),
            .. ofOthers,
        ]);

        LostItemRecentSummaryResponse result = await this.SummarizeAsync(classId);

        result.Count.Should().Be(ofOthers.Length);
    }

    [Test]
    public async Task count_the_items_of_exactly_the_last_thirty_days_but_not_the_older_ones()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        TimeSpan window = TimeSpan.FromDays(SummaryPeriod.DefaultDays);
        LostItem[] inside =
        [
            LostItemMother.With(schoolId: schoolId, createdAt: Now - window),
            LostItemMother.ForClass(classId, createdAt: Now - window),
        ];
        this.ShouldSearchLostItems(
        [
            .. inside,
            LostItemMother.With(schoolId: schoolId, createdAt: Now - window - JUST_OUTSIDE),
            LostItemMother.ForClass(classId, createdAt: Now - window - JUST_OUTSIDE),
        ]);

        LostItemRecentSummaryResponse result = await this.SummarizeAsync(classId);

        result.Count.Should().Be(inside.Length);
    }

    [Test]
    public async Task look_back_only_as_many_days_as_requested()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        TimeSpan window = TimeSpan.FromDays(REQUESTED_DAYS);
        LostItem[] inside =
        [
            LostItemMother.With(schoolId: schoolId, createdAt: Now - window),
            LostItemMother.ForClass(classId, createdAt: Now - window),
        ];
        this.ShouldSearchLostItems(
        [
            .. inside,
            LostItemMother.With(schoolId: schoolId, createdAt: Now - window - JUST_OUTSIDE),
            LostItemMother.ForClass(classId, createdAt: Now - window - JUST_OUTSIDE),
        ]);

        LostItemRecentSummaryResponse result = await this._useCase.ExecuteAsync(
            LostItemRecentSummaryFinderByClassQueryMother.WithDays(classId, REQUESTED_DAYS),
            CancellationToken.None);

        result.Count.Should().Be(inside.Length);
    }

    [TestCase(SummaryPeriod.MinDays - ONE_DAY)]
    [TestCase(-REQUESTED_DAYS)]
    [TestCase(SummaryPeriod.MaxDays + ONE_DAY)]
    public async Task throw_invalid_days_without_reading_when_the_period_is_out_of_range(int days)
    {
        (Guid classId, _) = this.ShouldResolveClassOfCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemRecentSummaryFinderByClassQueryMother.WithDays(classId, days),
            CancellationToken.None);

        await act.Should().ThrowAsync<SummaryPeriodExceptions.InvalidDays>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task not_count_the_items_of_other_classes_or_other_schools()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        LostItem[] mine = [LostItemMother.With(schoolId: schoolId, createdAt: Now)];
        this.ShouldSearchLostItems(
        [
            .. mine,
            LostItemMother.With(schoolId: GuidMother.Random(), createdAt: Now),
            LostItemMother.ForClass(GuidMother.Random(), createdAt: Now),
        ]);

        LostItemRecentSummaryResponse result = await this.SummarizeAsync(classId);

        result.Count.Should().Be(mine.Length);
    }

    [Test]
    public async Task return_zero_when_there_are_no_items()
    {
        (Guid classId, _) = this.ShouldResolveClassOfCurrentUser();
        this.ShouldSearchNoLostItems();

        LostItemRecentSummaryResponse result = await this.SummarizeAsync(classId);

        result.Should().Be(LostItemRecentSummaryResponseMother.None());
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

    [Test]
    public async Task throw_the_school_error_without_reading_when_the_school_of_the_class_cannot_be_found()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailFindingSchoolOfSchoolYear(schoolYearId, new SchoolYearExceptions.NotFound());
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);

        Func<Task> act = () => this.SummarizeAsync(classId);

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.ShouldNotHaveSearched();
    }

    private Task<LostItemRecentSummaryResponse> SummarizeAsync(Guid classId)
        => this._useCase.ExecuteAsync(
            LostItemRecentSummaryFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

    private (Guid ClassId, Guid SchoolId) ShouldResolveClassOfCurrentUser(Guid? userId = null)
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid currentUserId = userId ?? GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        return (classId, schoolId);
    }
}

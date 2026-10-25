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
public class LostItemOpenFinderByClassShould : LostItemsUnitTestCase
{
    private const int NONE = 0;
    private const int LONG_AGO_DAYS = 400;

    private static readonly TimeSpan LONG_AGO = TimeSpan.FromDays(LONG_AGO_DAYS);

    private readonly LostItemOpenFinderByClass.Finder _useCase;

    public LostItemOpenFinderByClassShould()
    {
        this._useCase = new LostItemOpenFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task count_the_unrecovered_items_of_the_school_and_of_the_class()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        LostItem[] open =
        [
            LostItemMother.With(schoolId: schoolId),
            LostItemMother.With(schoolId: schoolId, status: ItemStatus.Found),
            LostItemMother.ForClass(classId),
        ];
        this.ShouldSearchLostItems(open);

        LostItemOpenResponse result = await this.CountOpenAsync(classId);

        result.Count.Should().Be(open.Length);
    }

    [Test]
    public async Task not_count_the_items_already_recovered()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        LostItem[] open = [LostItemMother.With(schoolId: schoolId)];
        this.ShouldSearchLostItems(
        [
            .. open,
            LostItemMother.With(schoolId: schoolId, recovered: true),
            LostItemMother.ForClass(classId, recovered: true),
        ]);

        LostItemOpenResponse result = await this.CountOpenAsync(classId);

        result.Count.Should().Be(open.Length);
    }

    [Test]
    public async Task count_the_items_the_current_user_posted_as_well_as_the_ones_of_others()
    {
        Guid currentUserId = GuidMother.Random();
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser(currentUserId);
        LostItem[] open =
        [
            LostItemMother.With(schoolId: schoolId, authorId: currentUserId),
            LostItemMother.With(scope: ItemScope.Class, classId: classId, authorId: currentUserId),
            LostItemMother.With(schoolId: schoolId, authorId: GuidMother.Random()),
            LostItemMother.With(scope: ItemScope.Class, classId: classId, authorId: GuidMother.Random()),
        ];
        this.ShouldSearchLostItems(open);

        LostItemOpenResponse result = await this.CountOpenAsync(classId);

        result.Count.Should().Be(open.Length);
    }

    [Test]
    public async Task count_the_unrecovered_items_however_long_ago_they_were_posted()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        LostItem[] open =
        [
            LostItemMother.With(schoolId: schoolId, createdAt: Now - LONG_AGO),
            LostItemMother.ForClass(classId, createdAt: Now - LONG_AGO),
        ];
        this.ShouldSearchLostItems(open);

        LostItemOpenResponse result = await this.CountOpenAsync(classId);

        result.Count.Should().Be(open.Length);
    }

    [Test]
    public async Task not_count_the_items_of_other_classes_or_other_schools()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        LostItem[] mine = [LostItemMother.With(schoolId: schoolId)];
        this.ShouldSearchLostItems(
        [
            .. mine,
            LostItemMother.With(schoolId: GuidMother.Random()),
            LostItemMother.ForClass(GuidMother.Random()),
        ]);

        LostItemOpenResponse result = await this.CountOpenAsync(classId);

        result.Count.Should().Be(mine.Length);
    }

    [Test]
    public async Task return_zero_when_there_are_no_items()
    {
        (Guid classId, _) = this.ShouldResolveClassOfCurrentUser();
        this.ShouldSearchNoLostItems();

        LostItemOpenResponse result = await this.CountOpenAsync(classId);

        result.Should().Be(new LostItemOpenResponse(Count: NONE));
    }

    [Test]
    public async Task throw_unauthenticated_without_asking_or_reading_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this.CountOpenAsync(GuidMother.Random());

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

        Func<Task> act = () => this.CountOpenAsync(classId);

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_the_class_error_when_the_class_is_unknown()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldNotFindClass();

        Func<Task> act = () => this.CountOpenAsync(GuidMother.Random());

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

        Func<Task> act = () => this.CountOpenAsync(classId);

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.ShouldNotHaveSearched();
    }

    private Task<LostItemOpenResponse> CountOpenAsync(Guid classId)
        => this._useCase.ExecuteAsync(
            LostItemOpenFinderByClassQueryMother.WithClassId(classId),
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

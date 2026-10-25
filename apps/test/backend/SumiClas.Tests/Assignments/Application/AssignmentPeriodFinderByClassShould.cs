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
public class AssignmentPeriodFinderByClassShould : AssignmentsUnitTestCase
{
    private const int ONE_DAY = 1;
    private const int TWO_DAYS = 2;
    private const int WEEK_LENGTH_AFTER_FIRST_DAY = 6;

    private static readonly DateOnly FROM = new(2026, 10, 5);
    private static readonly DateOnly TO = FROM.AddDays(WEEK_LENGTH_AFTER_FIRST_DAY);

    private readonly AssignmentPeriodFinderByClass.Finder _useCase;

    public AssignmentPeriodFinderByClassShould()
    {
        this._useCase = new AssignmentPeriodFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task return_the_title_and_due_date_of_the_assignments_due_in_the_period_earliest_first()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        string later = StringMother.Random("later");
        string sooner = StringMother.Random("sooner");
        this.ShouldSearchAssignments(
            AssignmentMother.With(classId: classId, title: later, dueDate: FROM.AddDays(TWO_DAYS)),
            AssignmentMother.With(classId: classId, title: sooner, dueDate: FROM.AddDays(ONE_DAY)));

        IReadOnlyCollection<AssignmentPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Should().Equal(
            new AssignmentPeriodResponse(sooner, FROM.AddDays(ONE_DAY)),
            new AssignmentPeriodResponse(later, FROM.AddDays(TWO_DAYS)));
    }

    [Test]
    public async Task include_the_official_and_the_proposed_assignments()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        string official = StringMother.Random("official");
        string proposed = StringMother.Random("proposed");
        this.ShouldSearchAssignments(
            AssignmentMother.With(classId: classId, title: official, dueDate: FROM, isProposal: false),
            AssignmentMother.With(classId: classId, title: proposed, dueDate: TO, isProposal: true));

        IReadOnlyCollection<AssignmentPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Select(assignment => assignment.Title).Should().Equal(official, proposed);
    }

    [Test]
    public async Task order_the_assignments_due_the_same_day_by_title_comparing_characters_as_they_are()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        this.ShouldSearchAssignments(
            AssignmentMother.With(
                id: Guid.Parse("00000000-0000-0000-0000-000000000001"),
                classId: classId,
                title: "apple",
                dueDate: FROM),
            AssignmentMother.With(
                id: Guid.Parse("ff000000-0000-0000-0000-000000000000"),
                classId: classId,
                title: "Banana",
                dueDate: FROM));

        IReadOnlyCollection<AssignmentPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Select(assignment => assignment.Title).Should().Equal("Banana", "apple");
    }

    [Test]
    public async Task include_what_is_due_on_the_first_and_on_the_last_day_but_not_the_days_around()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        string first = StringMother.Random("first");
        string last = StringMother.Random("last");
        this.ShouldSearchAssignments(
            AssignmentMother.With(classId: classId, dueDate: FROM.AddDays(-ONE_DAY)),
            AssignmentMother.With(classId: classId, title: first, dueDate: FROM),
            AssignmentMother.With(classId: classId, title: last, dueDate: TO),
            AssignmentMother.With(classId: classId, dueDate: TO.AddDays(ONE_DAY)));

        IReadOnlyCollection<AssignmentPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Select(assignment => assignment.Title).Should().Equal(first, last);
    }

    [Test]
    public async Task give_the_same_answer_to_whoever_asks_whether_they_wrote_or_did_the_assignments_or_not()
    {
        Guid currentUserId = GuidMother.Random();
        Guid classId = this.ShouldResolveClassOfCurrentUser(currentUserId);
        string written = StringMother.Random("written");
        string done = StringMother.Random("done");
        this.ShouldSearchAssignments(
            AssignmentMother.With(classId: classId, title: written, dueDate: FROM, authorId: currentUserId),
            AssignmentMother.DoneBy(
                GuidMother.Random(), currentUserId, classId, FROM.AddDays(ONE_DAY), title: done));

        IReadOnlyCollection<AssignmentPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Select(assignment => assignment.Title).Should().Equal(written, done);
    }

    [Test]
    public async Task not_include_the_assignments_of_other_classes()
    {
        Guid classId = this.ShouldResolveClassOfCurrentUser();
        string mine = StringMother.Random("mine");
        this.ShouldSearchAssignments(
            AssignmentMother.With(classId: classId, title: mine, dueDate: FROM),
            AssignmentMother.With(classId: GuidMother.Random(), dueDate: FROM));

        IReadOnlyCollection<AssignmentPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Select(assignment => assignment.Title).Should().Equal(mine);
    }

    [Test]
    public async Task throw_class_access_forbidden_without_reading_when_the_user_does_not_belong_to_the_class()
    {
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldDenyClassAccess(currentUserId, classId);

        Func<Task> act = () => this.InPeriodAsync(classId);

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_the_class_error_when_the_class_is_unknown()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldNotFindClass();

        Func<Task> act = () => this.InPeriodAsync(GuidMother.Random());

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.ShouldNotHaveSearched();
    }

    private Task<IReadOnlyCollection<AssignmentPeriodResponse>> InPeriodAsync(Guid classId)
        => this._useCase.ExecuteAsync(
            AssignmentPeriodFinderByClassQueryMother.With(classId, FROM, TO),
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

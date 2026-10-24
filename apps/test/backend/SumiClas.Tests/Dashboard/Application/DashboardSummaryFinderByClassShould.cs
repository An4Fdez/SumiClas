namespace SumiClas.Tests.Dashboard.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Announcements.Application;
using SumiClas.Assignments.Application;
using SumiClas.CalendarEvents.Application;
using SumiClas.ClassExpenses.Application;
using SumiClas.Dashboard.Application;
using SumiClas.LostItems.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class DashboardSummaryFinderByClassShould : UnitTestCase
{
    private const int REQUESTED_DAYS = 7;
    private const int ONE_DAY = 1;

    private readonly DashboardSummaryFinderByClass.Finder _useCase;

    public DashboardSummaryFinderByClassShould()
    {
        this._useCase = new DashboardSummaryFinderByClass.Finder(this.QueryBus.Object);
    }

    [Test]
    public async Task gather_the_answer_of_every_module_in_one_summary()
    {
        Guid classId = GuidMother.Random();
        AnnouncementUnreadSummaryResponse announcements = DashboardCountersMother.Announcements();
        AssignmentPendingSummaryResponse assignments = DashboardCountersMother.Assignments();
        ClassExpensePendingSummaryResponse expenses = DashboardCountersMother.Expenses();
        LostItemRecentSummaryResponse lostItems = DashboardCountersMother.LostItems();
        IReadOnlyCollection<CalendarEventUpcomingResponse> upcomingEvents = DashboardCountersMother.UpcomingEvents();
        this.ShouldAnswerEveryModule(announcements, assignments, expenses, lostItems, upcomingEvents);

        DashboardSummaryResponse result = await this._useCase.ExecuteAsync(
            DashboardSummaryFinderByClassQueryMother.WithDays(classId, REQUESTED_DAYS),
            CancellationToken.None);

        result.Should().Be(new DashboardSummaryResponse(
            REQUESTED_DAYS,
            announcements,
            assignments,
            expenses,
            lostItems,
            upcomingEvents));
    }

    [Test]
    public async Task ask_every_module_about_the_requested_class_and_the_period_only_where_it_applies()
    {
        Guid classId = GuidMother.Random();
        this.ShouldAnswerEveryModule();

        await this._useCase.ExecuteAsync(
            DashboardSummaryFinderByClassQueryMother.WithDays(classId, REQUESTED_DAYS),
            CancellationToken.None);

        this.ShouldHaveSent(new AnnouncementUnreadSummaryFinderByClass.Query(classId, REQUESTED_DAYS));
        this.ShouldHaveSent(new AssignmentPendingSummaryFinderByClass.Query(classId));
        this.ShouldHaveSent(new ClassExpensePendingSummaryFinderByClass.Query(classId));
        this.ShouldHaveSent(new LostItemRecentSummaryFinderByClass.Query(classId, REQUESTED_DAYS));
        this.ShouldHaveSent(new CalendarEventUpcomingFinderByClass.Query(classId, REQUESTED_DAYS));
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task use_the_default_period_when_none_is_given()
    {
        Guid classId = GuidMother.Random();
        this.ShouldAnswerEveryModule();

        DashboardSummaryResponse result = await this._useCase.ExecuteAsync(
            DashboardSummaryFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

        result.Days.Should().Be(SummaryPeriod.DefaultDays);
        this.ShouldHaveSent(new AnnouncementUnreadSummaryFinderByClass.Query(classId, SummaryPeriod.DefaultDays));
        this.ShouldHaveSent(new LostItemRecentSummaryFinderByClass.Query(classId, SummaryPeriod.DefaultDays));
        this.ShouldHaveSent(new CalendarEventUpcomingFinderByClass.Query(classId, SummaryPeriod.DefaultDays));
    }

    [TestCase(SummaryPeriod.MinDays - ONE_DAY)]
    [TestCase(-REQUESTED_DAYS)]
    [TestCase(SummaryPeriod.MaxDays + ONE_DAY)]
    public async Task throw_invalid_days_without_asking_any_module_when_the_period_is_out_of_range(int days)
    {
        Func<Task> act = () => this._useCase.ExecuteAsync(
            DashboardSummaryFinderByClassQueryMother.WithDays(GuidMother.Random(), days),
            CancellationToken.None);

        await act.Should().ThrowAsync<SummaryPeriodExceptions.InvalidDays>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task fail_without_asking_the_rest_when_the_first_module_refuses_the_user()
    {
        this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<AnnouncementUnreadSummaryFinderByClass.Query>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UserExceptions.ClassAccessForbidden());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            DashboardSummaryFinderByClassQueryMother.WithClassId(GuidMother.Random()),
            CancellationToken.None);

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        this.QueryBus.Verify(
            bus => bus.Send(
                It.IsAny<AssignmentPendingSummaryFinderByClass.Query>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task fail_instead_of_returning_a_partial_summary_when_the_last_module_fails()
    {
        this.ShouldAnswerEveryModule();
        this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<CalendarEventUpcomingFinderByClass.Query>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            DashboardSummaryFinderByClassQueryMother.WithClassId(GuidMother.Random()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private void ShouldAnswerEveryModule(
        AnnouncementUnreadSummaryResponse? announcements = null,
        AssignmentPendingSummaryResponse? assignments = null,
        ClassExpensePendingSummaryResponse? expenses = null,
        LostItemRecentSummaryResponse? lostItems = null,
        IReadOnlyCollection<CalendarEventUpcomingResponse>? upcomingEvents = null)
    {
        this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<AnnouncementUnreadSummaryFinderByClass.Query>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(announcements ?? DashboardCountersMother.Announcements());
        this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<AssignmentPendingSummaryFinderByClass.Query>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignments ?? DashboardCountersMother.Assignments());
        this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<ClassExpensePendingSummaryFinderByClass.Query>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses ?? DashboardCountersMother.Expenses());
        this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<LostItemRecentSummaryFinderByClass.Query>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lostItems ?? DashboardCountersMother.LostItems());
        this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<CalendarEventUpcomingFinderByClass.Query>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(upcomingEvents ?? DashboardCountersMother.UpcomingEvents());
    }

    private void ShouldHaveSent<TResponse>(IQuery<TResponse> query)
        => this.QueryBus.Verify(bus => bus.Send(query, It.IsAny<CancellationToken>()), Times.Once);
}

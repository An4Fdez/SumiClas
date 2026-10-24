namespace SumiClas.Tests.WeeklySummaries.Application;

using AwesomeAssertions;
using SumiClas.WeeklySummaries.Application;

[TestFixture]
public class WeeklySummaryInputShould
{
    private const int SOME_DAY = 0;
    private const int SOME_COUNT = 1;
    private const int NEGATIVE = -1;

    [Test]
    public void be_empty_when_the_week_has_nothing()
    {
        WeeklySummaryInputMother.Empty().IsEmpty.Should().BeTrue();
    }

    [Test]
    public void be_empty_when_the_counts_are_negative()
    {
        WeeklySummaryInputMother.With(birthdays: NEGATIVE, unrecoveredLostItems: NEGATIVE).IsEmpty.Should().BeTrue();
    }

    [TestCaseSource(nameof(NonEmptyWeeks))]
    public void not_be_empty_when_the_week_has_anything(WeeklySummaryInput input)
    {
        input.IsEmpty.Should().BeFalse();
    }

    private static IEnumerable<TestCaseData> NonEmptyWeeks()
    {
        yield return new TestCaseData(WeeklySummaryInputMother.With(
            announcements: [WeeklySummaryInputMother.Announcement()])).SetName("an_announcement");
        yield return new TestCaseData(WeeklySummaryInputMother.With(
            events: [WeeklySummaryInputMother.Event(SOME_DAY)])).SetName("an_event");
        yield return new TestCaseData(WeeklySummaryInputMother.With(
            assignments: [WeeklySummaryInputMother.Assignment(SOME_DAY)])).SetName("an_assignment");
        yield return new TestCaseData(WeeklySummaryInputMother.With(
            pendingExpenses: ["expense"])).SetName("a_pending_expense");
        yield return new TestCaseData(WeeklySummaryInputMother.With(
            birthdays: SOME_COUNT)).SetName("a_birthday");
        yield return new TestCaseData(WeeklySummaryInputMother.With(
            unrecoveredLostItems: SOME_COUNT)).SetName("a_lost_item");
    }
}

namespace SumiClas.Tests.Shared;

using AwesomeAssertions;
using SumiClas.Shared.Domain;

[TestFixture]
public class SummaryPeriodShould
{
    private const int CONTRACT_MIN_DAYS = 1;
    private const int CONTRACT_DEFAULT_DAYS = 30;
    private const int CONTRACT_MAX_DAYS = 90;
    private const int A_WEEK = 7;
    private const int ONE_DAY = 1;

    [Test]
    public void resolve_a_missing_period_to_the_default()
    {
        SummaryPeriod.Resolve(null).Should().Be(SummaryPeriod.DefaultDays);
    }

    [TestCase(SummaryPeriod.MinDays)]
    [TestCase(A_WEEK)]
    [TestCase(SummaryPeriod.MaxDays)]
    public void keep_a_period_inside_the_bounds(int days)
    {
        SummaryPeriod.Resolve(days).Should().Be(days);
    }

    [TestCase(SummaryPeriod.MinDays - ONE_DAY)]
    [TestCase(-A_WEEK)]
    [TestCase(SummaryPeriod.MaxDays + ONE_DAY)]
    public void throw_invalid_days_for_a_period_outside_the_bounds(int days)
    {
        Action act = () => SummaryPeriod.Resolve(days);

        act.Should().Throw<SummaryPeriodExceptions.InvalidDays>();
    }

    [Test]
    public void name_the_bounds_in_the_error_message()
    {
        string message = new SummaryPeriodExceptions.InvalidDays().Message;

        message.Should().Contain(SummaryPeriod.MinDays.ToString()).And.Contain(SummaryPeriod.MaxDays.ToString());
    }

    [Test]
    public void keep_the_agreed_minimum_default_and_maximum()
    {
        SummaryPeriod.MinDays.Should().Be(CONTRACT_MIN_DAYS);
        SummaryPeriod.DefaultDays.Should().Be(CONTRACT_DEFAULT_DAYS);
        SummaryPeriod.MaxDays.Should().Be(CONTRACT_MAX_DAYS);
    }
}

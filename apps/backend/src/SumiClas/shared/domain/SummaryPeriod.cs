namespace SumiClas.Shared.Domain;

/// <summary>How far back the "news" counters of the dashboard look, chosen by the user and bounded.</summary>
public static class SummaryPeriod
{
    public const int MinDays = 1;
    public const int DefaultDays = 30;
    public const int MaxDays = 90;

    /// <summary>Validates the period of a request; a null value means <see cref="DefaultDays"/>.</summary>
    public static int Resolve(int? days)
    {
        int resolved = days ?? DefaultDays;
        return resolved is < MinDays or > MaxDays ? throw new SummaryPeriodExceptions.InvalidDays() : resolved;
    }
}

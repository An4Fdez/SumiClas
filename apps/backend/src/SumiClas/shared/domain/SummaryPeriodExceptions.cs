namespace SumiClas.Shared.Domain;

public static class SummaryPeriodExceptions
{
    public sealed class InvalidDays() : DomainException(
        "SummaryPeriod.InvalidDays",
        $"El periodo debe ser de entre {SummaryPeriod.MinDays} y {SummaryPeriod.MaxDays} días.");
}

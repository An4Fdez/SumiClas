namespace SumiClas.Tests.WeeklySummaries.Domain;

using SumiClas.Tests.Shared;
using SumiClas.WeeklySummaries.Domain;
using System.Security.Cryptography;

public static class WeeklySummaryMother
{
    private const int HASH_BYTES = WeeklySummary.InputHashLength / 2;

    /// <summary>A Monday: the week of the clock of the tests starts on it.</summary>
    public static readonly DateOnly Monday = new(2026, 10, 5);

    public static WeeklySummary With(
        Guid? id = null,
        Guid? classId = null,
        DateOnly? weekStart = null,
        string? content = null,
        string? inputHash = null,
        DateTimeOffset? generatedAt = null)
        => new(
            id ?? GuidMother.Random(),
            classId ?? GuidMother.Random(),
            weekStart ?? Monday,
            content ?? StringMother.Random("summary"),
            inputHash ?? InputHash(),
            generatedAt ?? UnitTestCase.Now);

    /// <summary>A fingerprint as long as the real ones (a SHA-256 in hexadecimal).</summary>
    public static string InputHash() => Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(HASH_BYTES));
}

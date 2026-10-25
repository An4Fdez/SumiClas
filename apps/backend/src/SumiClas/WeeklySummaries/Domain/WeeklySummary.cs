using SumiClas.Shared.Domain;

namespace SumiClas.WeeklySummaries.Domain;

/// <summary>
/// The summary of one week of a class, written once and read by every family of the class. A class has at most one per
/// week, which is what makes generating it again harmless. It remembers a fingerprint of the data it was written from, so
/// whoever reads it can tell whether the week has changed since and the text needs to be written again.
/// </summary>
public sealed class WeeklySummary : AggregateRoot
{
    /// <summary>A SHA-256 in hexadecimal.</summary>
    public const int InputHashLength = 64;

    private const int DaysInWeek = 7;

    public Guid ClassId { get; private set; }

    /// <summary>The Monday the week starts on.</summary>
    public DateOnly WeekStart { get; private set; }

    public string Content { get; private set; }

    /// <summary>The fingerprint of the data the content was written from.</summary>
    public string InputHash { get; private set; }

    public DateTimeOffset GeneratedAt { get; private set; }

    public WeeklySummary(
        Guid id,
        Guid classId,
        DateOnly weekStart,
        string content,
        string inputHash,
        DateTimeOffset generatedAt) : base(id)
    {
        ClassId = classId;
        WeekStart = weekStart;
        Content = content;
        InputHash = inputHash;
        GeneratedAt = generatedAt;
    }

    public static WeeklySummary Create(
        Guid id,
        Guid classId,
        DateOnly weekStart,
        string content,
        string inputHash,
        IClock clock)
    {
        if (id == Guid.Empty)
        {
            throw new WeeklySummaryExceptions.IdRequired();
        }

        if (classId == Guid.Empty)
        {
            throw new WeeklySummaryExceptions.ClassRequired();
        }

        if (weekStart.DayOfWeek != DayOfWeek.Monday)
        {
            throw new WeeklySummaryExceptions.WeekStartInvalid();
        }

        ValidateWrittenText(content, inputHash);

        return new WeeklySummary(id, classId, weekStart, content.Trim(), inputHash, clock.UtcNow);
    }

    /// <summary>Replaces the text with one written from newer data: it is the same summary, now generated again.</summary>
    public void Rewrite(string content, string inputHash, IClock clock)
    {
        ValidateWrittenText(content, inputHash);

        Content = content.Trim();
        InputHash = inputHash;
        GeneratedAt = clock.UtcNow;
    }

    public bool WasWrittenFrom(string inputHash) => InputHash == inputHash;

    /// <summary>The Monday of the week that <paramref name="day"/> belongs to (a Sunday belongs to the week before).</summary>
    public static DateOnly WeekStartOf(DateOnly day)
    {
        int daysSinceMonday = ((int)day.DayOfWeek + DaysInWeek - (int)DayOfWeek.Monday) % DaysInWeek;
        return day.AddDays(-daysSinceMonday);
    }

    /// <summary>The Sunday of the week that starts on <paramref name="weekStart"/>.</summary>
    public static DateOnly WeekEndOf(DateOnly weekStart) => weekStart.AddDays(DaysInWeek - 1);

    private static void ValidateWrittenText(string content, string inputHash)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new WeeklySummaryExceptions.ContentRequired();
        }

        if (string.IsNullOrWhiteSpace(inputHash))
        {
            throw new WeeklySummaryExceptions.InputHashRequired();
        }
    }
}

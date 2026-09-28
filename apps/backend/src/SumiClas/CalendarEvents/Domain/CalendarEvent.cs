using SumiClas.Shared.Domain;

namespace SumiClas.CalendarEvents.Domain;

public sealed class CalendarEvent : AggregateRoot
{
    public EventScope Scope { get; private set; }

    /// <summary>Set only for non-Birthday events with Scope == Class.</summary>
    public Guid? ClassId { get; private set; }

    /// <summary>
    /// Set only for Birthday events. A birthday follows the (permanent) Student, never a
    /// fixed Class — Classes are recreated every SchoolYear, so a Class reference would
    /// keep showing the birthday in last year's class forever instead of following the
    /// child to their new one. Which class to display it in is resolved dynamically
    /// (via the student's current Enrollment), not stored here.
    /// </summary>
    public Guid? StudentId { get; private set; }

    public EventType Type { get; private set; }
    public string Title { get; private set; }
    public EventDate EventDate { get; private set; }
    public TimeOnly? Time { get; private set; }
    public string? Location { get; private set; }

    /// <summary>Informational text only, never a real payment status (that lives in Expenses).</summary>
    public string? Cost { get; private set; }

    public Guid AuthorId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private CalendarEvent(
        Guid id,
        EventScope scope,
        Guid? classId,
        Guid? studentId,
        EventType type,
        string title,
        EventDate eventDate,
        TimeOnly? time,
        string? location,
        string? cost,
        Guid authorId,
        DateTimeOffset createdAt) : base(id)
    {
        Scope = scope;
        ClassId = classId;
        StudentId = studentId;
        Type = type;
        Title = title;
        EventDate = eventDate;
        Time = time;
        Location = location;
        Cost = cost;
        AuthorId = authorId;
        CreatedAt = createdAt;
    }

    /// <summary>EF Core materialization only — constructor parameters can never bind to an owned-type navigation (EventDate), even when named to match.</summary>
    private CalendarEvent()
    {
        Title = null!;
        EventDate = null!;
    }

    /// <summary>For Trip, Meeting, Activity and Other — one-time events on a real date. Use CreateBirthday for birthdays.</summary>
    public static Result<CalendarEvent> Create(
        EventScope scope,
        Guid? classId,
        EventType type,
        string title,
        DateOnly date,
        TimeOnly? time,
        string? location,
        string? cost,
        Guid authorId,
        IClock clock)
    {
        var validation = Validate(scope, classId, type, title, authorId);
        if (validation.IsFailure)
        {
            return Result<CalendarEvent>.Failure(validation.Error!);
        }

        var eventDate = EventDate.OnDate(date);

        var calendarEvent = new CalendarEvent(
            Guid.NewGuid(),
            scope,
            scope == EventScope.Class ? classId : null,
            null,
            type,
            title.Trim(),
            eventDate.Value,
            time,
            NormalizeOptional(location),
            NormalizeOptional(cost),
            authorId,
            clock.UtcNow);

        return Result<CalendarEvent>.Success(calendarEvent);
    }

    private static Result Validate(EventScope scope, Guid? classId, EventType type, string title, Guid authorId)
    {
        if (type == EventType.Birthday)
        {
            return Result.Failure(CalendarEventErrors.UseCreateBirthdayForBirthdays);
        }
        if (scope == EventScope.Class && (classId is null || classId == Guid.Empty))
        {
            return Result.Failure(CalendarEventErrors.ClassRequiredForClassScope);
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(CalendarEventErrors.TitleRequired);
        }
        if (authorId == Guid.Empty)
        {
            return Result.Failure(CalendarEventErrors.AuthorRequired);
        }
        return Result.Success();
    }

    /// <summary>
    /// Birthdays are always Class-scoped (via the student's current class, resolved
    /// dynamically) and always recurring (month+day, never a year — see EventDate/StudentId).
    /// </summary>
    public static Result<CalendarEvent> CreateBirthday(
        Guid studentId,
        string title,
        int month,
        int day,
        Guid authorId,
        IClock clock)
    {
        var validation = ValidateBirthday(studentId, title, authorId);
        if (validation.IsFailure)
        {
            return Result<CalendarEvent>.Failure(validation.Error!);
        }

        var eventDate = EventDate.RecurringAnnual(month, day);
        if (eventDate.IsFailure)
        {
            return Result<CalendarEvent>.Failure(eventDate.Error!);
        }

        var birthday = new CalendarEvent(
            Guid.NewGuid(),
            EventScope.Class,
            null,
            studentId,
            EventType.Birthday,
            title.Trim(),
            eventDate.Value,
            null,
            null,
            null,
            authorId,
            clock.UtcNow);

        return Result<CalendarEvent>.Success(birthday);
    }

    private static Result ValidateBirthday(Guid studentId, string title, Guid authorId)
    {
        if (studentId == Guid.Empty)
        {
            return Result.Failure(CalendarEventErrors.StudentRequiredForBirthday);
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(CalendarEventErrors.TitleRequired);
        }
        if (authorId == Guid.Empty)
        {
            return Result.Failure(CalendarEventErrors.AuthorRequired);
        }
        return Result.Success();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

using SumiClas.Shared.Domain;

namespace SumiClas.CalendarEvents.Domain;

public sealed class CalendarEvent : AggregateRoot
{
    public EventScope Scope { get; private set; }

    /// <summary>Set only for non-Birthday events with Scope == Class.</summary>
    public Guid? ClassId { get; private set; }

    /// <summary>Set only for non-Birthday events with Scope == School: the school the event belongs to.</summary>
    public Guid? SchoolId { get; private set; }

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

    /// <summary>When the author last modified it; null if it was never modified.</summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

    public CalendarEvent(
        Guid id,
        EventScope scope,
        Guid? classId,
        Guid? schoolId,
        Guid? studentId,
        EventType type,
        string title,
        EventDate eventDate,
        TimeOnly? time,
        string? location,
        string? cost,
        Guid authorId,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt) : base(id)
    {
        Scope = scope;
        ClassId = classId;
        SchoolId = schoolId;
        StudentId = studentId;
        Type = type;
        Title = title;
        EventDate = eventDate;
        Time = time;
        Location = location;
        Cost = cost;
        AuthorId = authorId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    private CalendarEvent()
    {
        // For reflection purposes (EF Core materialization)
    }

    /// <summary>For Trip, Meeting, Activity and Other — one-time events on a real date. Use CreateBirthday for birthdays.</summary>
    public static CalendarEvent Create(
        Guid id,
        EventScope scope,
        Guid? classId,
        Guid? schoolId,
        EventType type,
        string title,
        DateOnly date,
        TimeOnly? time,
        string? location,
        string? cost,
        Guid authorId,
        IClock clock)
    {
        Validate(id, scope, classId, schoolId, type, title, authorId);

        return new CalendarEvent(
            id,
            scope,
            scope == EventScope.Class ? classId : null,
            scope == EventScope.School ? schoolId : null,
            null,
            type,
            title.Trim(),
            EventDate.OnDate(date),
            time,
            NormalizeOptional(location),
            NormalizeOptional(cost),
            authorId,
            clock.UtcNow,
            null);
    }

    private static void Validate(Guid id, EventScope scope, Guid? classId, Guid? schoolId, EventType type, string title, Guid authorId)
    {
        if (id == Guid.Empty)
        {
            throw new CalendarEventExceptions.IdRequired();
        }
        if (type == EventType.Birthday)
        {
            throw new CalendarEventExceptions.UseCreateBirthdayForBirthdays();
        }
        if (scope == EventScope.Class && (classId is null || classId == Guid.Empty))
        {
            throw new CalendarEventExceptions.ClassRequiredForClassScope();
        }
        if (scope == EventScope.School && (schoolId is null || schoolId == Guid.Empty))
        {
            throw new CalendarEventExceptions.SchoolRequiredForSchoolScope();
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new CalendarEventExceptions.TitleRequired();
        }
        if (authorId == Guid.Empty)
        {
            throw new CalendarEventExceptions.AuthorRequired();
        }
    }

    /// <summary>Changes what the author may change. A birthday is never changed: it is deleted and created again.</summary>
    public void Update(EventType type, string title, DateOnly date, TimeOnly? time, string? location, string? cost, IClock clock)
    {
        if (Type == EventType.Birthday)
        {
            throw new CalendarEventExceptions.BirthdayChangeInvalid();
        }

        ValidateContent(type, title);

        Type = type;
        Title = title.Trim();
        EventDate = EventDate.OnDate(date);
        Time = time;
        Location = NormalizeOptional(location);
        Cost = NormalizeOptional(cost);
        UpdatedAt = clock.UtcNow;
    }

    public static void ValidateContent(EventType type, string title)
    {
        if (type == EventType.Birthday)
        {
            throw new CalendarEventExceptions.UseCreateBirthdayForBirthdays();
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new CalendarEventExceptions.TitleRequired();
        }
    }

    /// <summary>
    /// Birthdays are always Class-scoped (via the student's current class, resolved
    /// dynamically) and always recurring (month+day, never a year — see EventDate/StudentId).
    /// </summary>
    public static CalendarEvent CreateBirthday(
        Guid id,
        Guid studentId,
        string title,
        int month,
        int day,
        Guid authorId,
        IClock clock)
    {
        ValidateBirthday(id, studentId, title, authorId);

        EventDate eventDate = EventDate.RecurringAnnual(month, day);

        return new CalendarEvent(
            id,
            EventScope.Class,
            null,
            null,
            studentId,
            EventType.Birthday,
            title.Trim(),
            eventDate,
            null,
            null,
            null,
            authorId,
            clock.UtcNow,
            null);
    }

    private static void ValidateBirthday(Guid id, Guid studentId, string title, Guid authorId)
    {
        if (id == Guid.Empty)
        {
            throw new CalendarEventExceptions.IdRequired();
        }
        if (studentId == Guid.Empty)
        {
            throw new CalendarEventExceptions.StudentRequiredForBirthday();
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new CalendarEventExceptions.TitleRequired();
        }
        if (authorId == Guid.Empty)
        {
            throw new CalendarEventExceptions.AuthorRequired();
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

using SumiClas.Shared.Domain;

namespace SumiClas.CalendarEvents.Domain;

public sealed class CalendarEventSpecification : Specification<CalendarEvent>
{
    /// <param name="eventMonth">Month of the event date.</param>
    /// <param name="eventDay">Day of the event date.</param>
    /// <param name="eventYear">Year of the event date; birthdays have none, so this never matches them.</param>
    public CalendarEventSpecification(
        Guid? id = null,
        EventScope? scope = null,
        Guid? classId = null,
        Guid? studentId = null,
        EventType? type = null,
        string? title = null,
        int? eventMonth = null,
        int? eventDay = null,
        int? eventYear = null,
        TimeOnly? time = null,
        string? location = null,
        string? cost = null,
        Guid? authorId = null,
        DateTimeOffset? createdAt = null)
    {
        if (id is { } eventId) AddCriteria(calendarEvent => calendarEvent.Id == eventId);
        if (scope is { } eventScope) AddCriteria(calendarEvent => calendarEvent.Scope == eventScope);
        if (classId is { } schoolClass) AddCriteria(calendarEvent => calendarEvent.ClassId == schoolClass);
        if (studentId is { } student) AddCriteria(calendarEvent => calendarEvent.StudentId == student);
        if (type is { } eventType) AddCriteria(calendarEvent => calendarEvent.Type == eventType);
        if (title is not null) AddCriteria(calendarEvent => calendarEvent.Title == title);
        if (eventMonth is { } month) AddCriteria(calendarEvent => calendarEvent.EventDate.Month == month);
        if (eventDay is { } day) AddCriteria(calendarEvent => calendarEvent.EventDate.Day == day);
        if (eventYear is { } year) AddCriteria(calendarEvent => calendarEvent.EventDate.Year == year);
        if (time is { } eventTime) AddCriteria(calendarEvent => calendarEvent.Time == eventTime);
        if (location is not null) AddCriteria(calendarEvent => calendarEvent.Location == location);
        if (cost is not null) AddCriteria(calendarEvent => calendarEvent.Cost == cost);
        if (authorId is { } author) AddCriteria(calendarEvent => calendarEvent.AuthorId == author);
        if (createdAt is { } created) AddCriteria(calendarEvent => calendarEvent.CreatedAt == created);
    }
}

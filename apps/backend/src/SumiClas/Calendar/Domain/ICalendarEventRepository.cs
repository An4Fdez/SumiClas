namespace SumiClas.Calendar.Domain;

public interface ICalendarEventRepository
{
    Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<CalendarEvent>> GetByClassIdAsync(Guid classId, CancellationToken ct = default);
    Task<IReadOnlyCollection<CalendarEvent>> GetSchoolWideAsync(CancellationToken ct = default);

    /// <summary>Birthdays for this student — the class they currently appear in is resolved separately via Enrollment, not stored on the event.</summary>
    Task<IReadOnlyCollection<CalendarEvent>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default);

    Task AddAsync(CalendarEvent calendarEvent, CancellationToken ct = default);
}

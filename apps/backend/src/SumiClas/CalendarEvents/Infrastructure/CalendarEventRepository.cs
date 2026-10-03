using SumiClas.CalendarEvents.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.CalendarEvents.Infrastructure;

public sealed class CalendarEventRepository(SumiClasDbContext dbContext)
    : BaseRepository<CalendarEvent>(dbContext), ICalendarEventRepository;

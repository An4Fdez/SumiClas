import { formatFullDay } from '@/lib/date';
import { EventAgendaRow } from '@/components/app/EventAgendaRow';
import type { CalendarEvent } from '@/data/calendarMock';

export function CalendarDayAgenda({
  day,
  events,
  classNameForEvent,
  onOpenEvent,
  showDate = true,
}: {
  day: Date | null;
  events: CalendarEvent[];
  classNameForEvent: (event: CalendarEvent) => string | undefined;
  onOpenEvent: (event: CalendarEvent) => void;
  showDate?: boolean;
}) {
  if (!day) return null;

  return (
    <div className={showDate ? 'mt-4' : ''}>
      {showDate && (
        <h2 className="font-display text-base font-bold text-slate-900 dark:text-white">{formatFullDay(day)}</h2>
      )}

      {events.length === 0 ? (
        <p className={`text-sm text-slate-500 dark:text-slate-400 ${showDate ? 'mt-2' : ''}`}>
          No hay eventos este día.
        </p>
      ) : (
        <div className={`space-y-2 ${showDate ? 'mt-3' : ''}`}>
          {events.map((event) => (
            <EventAgendaRow
              key={event.id}
              event={event}
              scopeLabel={event.scope === 'school' ? 'Colegio' : (classNameForEvent(event) ?? 'Clase')}
              onOpen={() => onOpenEvent(event)}
            />
          ))}
        </div>
      )}
    </div>
  );
}

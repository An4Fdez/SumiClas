import { badgeColorClasses } from '@/lib/colorClasses';
import type { CalendarDay } from '@/lib/date';
import { eventTypeMeta, type CalendarEvent } from '@/data/calendarMock';

export function CalendarDayCell({
  day,
  events,
  onOpen,
}: {
  day: CalendarDay;
  events: CalendarEvent[];
  onOpen: (date: Date) => void;
}) {
  const sortedEvents = [...events].sort((a, b) => (a.time ?? '99:99').localeCompare(b.time ?? '99:99'));
  const first = sortedEvents[0];
  const remaining = sortedEvents.length - 1;
  const firstColor = first ? badgeColorClasses[eventTypeMeta[first.type].color] : null;

  return (
    <button
      type="button"
      onClick={() => onOpen(day.date)}
      className={`flex min-h-[60px] w-full flex-col items-start gap-1 rounded-lg p-1.5 text-left transition-colors ${
        !day.inCurrentMonth ? 'opacity-40' : ''
      } ${day.isToday ? 'bg-brand-50 dark:bg-brand-900/20' : 'hover:bg-slate-100 dark:hover:bg-slate-800'}`}
    >
      <span
        className={`text-xs font-semibold ${
          day.isToday ? 'text-brand-700 dark:text-brand-300' : 'text-slate-700 dark:text-slate-300'
        }`}
      >
        {day.date.getDate()}
      </span>

      {first && firstColor && (
        <span
          className={`w-full truncate rounded px-1 py-0.5 text-[10px] font-semibold leading-tight ${firstColor.bg} ${firstColor.text}`}
        >
          {first.title}
        </span>
      )}
      {remaining > 0 && <span className="text-[10px] font-semibold text-slate-400 dark:text-slate-500">+{remaining}</span>}
    </button>
  );
}

import { formatMonthYear, generateMonthMatrix } from '@/lib/date';
import { CalendarDayCell } from '@/components/app/CalendarDayCell';
import { CalendarNavHeader } from '@/components/app/CalendarNavHeader';
import type { CalendarEvent } from '@/data/calendarMock';

const weekDays = ['L', 'M', 'X', 'J', 'V', 'S', 'D'];

export function CalendarGrid({
  year,
  month,
  eventsForDay,
  onChangeMonth,
  onToday,
  onOpenDay,
}: {
  year: number;
  month: number;
  eventsForDay: (day: Date) => CalendarEvent[];
  onChangeMonth: (delta: number) => void;
  onToday: () => void;
  onOpenDay: (day: Date) => void;
}) {
  const weeks = generateMonthMatrix(year, month);

  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-3 dark:border-slate-700 dark:bg-slate-900">
      <CalendarNavHeader
        title={formatMonthYear(year, month)}
        onPrev={() => onChangeMonth(-1)}
        onNext={() => onChangeMonth(1)}
        onToday={onToday}
      />

      <div className="mt-3 grid grid-cols-7 gap-1 text-center text-[11px] font-semibold text-slate-400 dark:text-slate-500">
        {weekDays.map((d) => (
          <div key={d}>{d}</div>
        ))}
      </div>

      <div className="mt-1 grid grid-cols-7 gap-1">
        {weeks.flatMap((week, wi) =>
          week.map((day, di) => (
            <CalendarDayCell key={`${wi}-${di}`} day={day} events={eventsForDay(day.date)} onOpen={onOpenDay} />
          ))
        )}
      </div>
    </div>
  );
}

import { formatWeekRange, generateWeek } from '@/lib/date';
import { CalendarDayCell } from '@/components/app/CalendarDayCell';
import { CalendarNavHeader } from '@/components/app/CalendarNavHeader';
import type { CalendarEvent } from '@/modules/calendarEvents/domain/CalendarEvent';

const weekDays = ['L', 'M', 'X', 'J', 'V', 'S', 'D'];

export function CalendarWeekGrid({
  referenceDate,
  eventsForDay,
  onChangeWeek,
  onToday,
  onOpenDay,
}: {
  referenceDate: Date;
  eventsForDay: (day: Date) => CalendarEvent[];
  onChangeWeek: (delta: number) => void;
  onToday: () => void;
  onOpenDay: (day: Date) => void;
}) {
  const days = generateWeek(referenceDate);

  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-3 dark:border-slate-700 dark:bg-slate-900">
      <CalendarNavHeader
        title={formatWeekRange(days)}
        onPrev={() => onChangeWeek(-1)}
        onNext={() => onChangeWeek(1)}
        onToday={onToday}
      />

      <div className="mt-3 grid grid-cols-7 gap-1 text-center text-[11px] font-semibold text-slate-400 dark:text-slate-500">
        {weekDays.map((d) => (
          <div key={d}>{d}</div>
        ))}
      </div>

      <div className="mt-1 grid grid-cols-7 gap-1">
        {days.map((day) => (
          <CalendarDayCell key={day.date.toISOString()} day={day} events={eventsForDay(day.date)} onOpen={onOpenDay} />
        ))}
      </div>
    </div>
  );
}

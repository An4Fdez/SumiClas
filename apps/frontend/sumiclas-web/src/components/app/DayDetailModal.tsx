import { useRef, type MouseEvent } from 'react';
import { X } from 'lucide-react';
import { CalendarDayAgenda } from '@/components/app/CalendarDayAgenda';
import type { CalendarEvent } from '@/modules/calendarEvents/domain/CalendarEvent';

export function DayDetailModal({
  day,
  events,
  classNameForEvent,
  onOpenEvent,
  onClose,
}: {
  day: Date;
  events: CalendarEvent[];
  classNameForEvent: (event: CalendarEvent) => string | undefined;
  onOpenEvent: (event: CalendarEvent) => void;
  onClose: () => void;
}) {
  const mouseDownOnBackdrop = useRef(false);

  return (
    <div
      className="fixed inset-0 z-50 flex items-end justify-center bg-slate-900/40 p-0 backdrop-blur-sm sm:items-center sm:p-4"
      onMouseDown={(e: MouseEvent) => {
        mouseDownOnBackdrop.current = e.target === e.currentTarget;
      }}
      onClick={(e: MouseEvent) => {
        if (mouseDownOnBackdrop.current && e.target === e.currentTarget) onClose();
      }}
    >
      <div
        className="max-h-[85vh] w-full max-w-lg overflow-y-auto rounded-t-3xl border border-slate-200 bg-white p-6 shadow-xl dark:border-slate-700 dark:bg-slate-900 sm:rounded-3xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-start justify-end">
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <CalendarDayAgenda
          day={day}
          events={events}
          classNameForEvent={classNameForEvent}
          onOpenEvent={onOpenEvent}
          showDate
        />
      </div>
    </div>
  );
}

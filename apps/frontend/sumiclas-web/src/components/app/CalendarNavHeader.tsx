import { ChevronLeft, ChevronRight } from 'lucide-react';

export function CalendarNavHeader({
  title,
  onPrev,
  onNext,
  onToday,
}: {
  title: string;
  onPrev: () => void;
  onNext: () => void;
  onToday: () => void;
}) {
  return (
    <div className="flex items-center justify-between">
      <button
        type="button"
        onClick={onPrev}
        aria-label="Anterior"
        className="rounded-full p-1.5 text-slate-500 transition-colors hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-800"
      >
        <ChevronLeft className="h-5 w-5" />
      </button>

      <div className="flex items-center gap-2">
        <span className="font-display text-base font-bold text-slate-900 dark:text-white">{title}</span>
        <button
          type="button"
          onClick={onToday}
          className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600 transition-colors hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700"
        >
          Hoy
        </button>
      </div>

      <button
        type="button"
        onClick={onNext}
        aria-label="Siguiente"
        className="rounded-full p-1.5 text-slate-500 transition-colors hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-800"
      >
        <ChevronRight className="h-5 w-5" />
      </button>
    </div>
  );
}

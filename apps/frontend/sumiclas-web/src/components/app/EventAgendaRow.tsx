import { badgeColorClasses } from '@/lib/colorClasses';
import { eventTypeMeta } from '@/lib/eventTypeMeta';
import type { CalendarEvent } from '@/modules/calendarEvents/domain/CalendarEvent';

export function EventAgendaRow({
  event,
  scopeLabel,
  onOpen,
}: {
  event: CalendarEvent;
  scopeLabel: string;
  onOpen: () => void;
}) {
  const meta = eventTypeMeta[event.type];
  const Icon = meta.icon;
  const c = badgeColorClasses[meta.color];

  return (
    <button
      type="button"
      onClick={onOpen}
      className="flex w-full items-center gap-3 rounded-2xl border border-slate-200 bg-white p-3 text-left shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md dark:border-slate-700 dark:bg-slate-900"
    >
      <div className={`grid h-9 w-9 shrink-0 place-items-center rounded-xl ${c.bg} ${c.text}`}>
        <Icon className="h-4 w-4" />
      </div>
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-bold text-slate-900 dark:text-white">{event.title}</p>
        <p className="truncate text-xs text-slate-500 dark:text-slate-400">
          {meta.label}
          {event.time ? ` · ${event.time}` : ''} · {scopeLabel}
        </p>
      </div>
    </button>
  );
}

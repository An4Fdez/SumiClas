import { useRef, type MouseEvent } from 'react';
import { Coins, MapPin, X } from 'lucide-react';
import { badgeColorClasses } from '@/lib/colorClasses';
import { formatFullDay, formatRelativeDate, monthNames, occurrenceInYear } from '@/lib/date';
import { eventTypeMeta } from '@/lib/eventTypeMeta';
import { roleLabel } from '@/lib/roles';
import type { CalendarEvent } from '@/modules/calendarEvents/domain/CalendarEvent';
import { AuthorActions } from './AuthorActions';

export function EventDetailModal({
  event,
  className,
  onClose,
  onEdit,
  onDelete,
}: {
  event: CalendarEvent;
  className?: string;
  onClose: () => void;
  onEdit: () => void;
  onDelete: () => Promise<void>;
}) {
  const meta = eventTypeMeta[event.type];
  const Icon = meta.icon;
  const c = badgeColorClasses[meta.color];
  const scopeLabel = event.scope === 'school' ? 'Colegio' : (className ?? 'Clase');
  const mouseDownOnBackdrop = useRef(false);

  const dateText =
    event.year === undefined
      ? `${event.day} de ${monthNames[event.month - 1].toLowerCase()} (cada año)`
      : formatFullDay(occurrenceInYear(event, event.year));

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
        <div className="flex items-start justify-between gap-3">
          <span
            className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-[11px] font-bold ${c.bg} ${c.text}`}
          >
            <Icon className="h-3 w-3" />
            {meta.label}
          </span>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <h2 className="mt-3 font-display text-xl font-extrabold text-slate-900 dark:text-white">{event.title}</h2>
        <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
          {[
            event.authorName,
            roleLabel(event.authorRole),
            scopeLabel,
            event.updatedAt && `editado ${formatRelativeDate(event.updatedAt)}`,
          ]
            .filter(Boolean)
            .join(' · ')}
        </p>

        <div className="mt-4 space-y-2 text-sm text-slate-700 dark:text-slate-300">
          <p>
            {dateText}
            {event.time ? ` · ${event.time}` : ''}
          </p>
          {event.location && (
            <p className="flex items-center gap-1.5">
              <MapPin className="h-4 w-4 shrink-0 text-slate-400" />
              {event.location}
            </p>
          )}
          {event.cost && (
            <p className="flex items-center gap-1.5">
              <Coins className="h-4 w-4 shrink-0 text-slate-400" />
              {event.cost}
            </p>
          )}
        </div>

        {event.isAuthor && (
          <AuthorActions
            deleteTitle={event.type === 'birthday' ? '¿Eliminar este cumpleaños?' : '¿Eliminar este evento?'}
            deleteMessage="Se borrará para todas las familias y no se puede deshacer."
            onEdit={event.type === 'birthday' ? undefined : onEdit}
            onDelete={onDelete}
          />
        )}
      </div>
    </div>
  );
}

import { useRef, type MouseEvent } from 'react';
import { AlertCircle, ArrowUpRight, X } from 'lucide-react';
import { Link } from 'react-router-dom';
import { badgeColorClasses } from '@/lib/colorClasses';
import { formatRelativeDate } from '@/lib/date';
import { roleLabel } from '@/lib/roles';
import type { Announcement } from '@/modules/announcements/domain/Announcement';
import { AuthorActions } from './AuthorActions';

export function AnnouncementDetailModal({
  announcement,
  className,
  onClose,
  onEdit,
  onDelete,
}: {
  announcement: Announcement;
  className?: string;
  onClose: () => void;
  onEdit: () => void;
  onDelete: () => Promise<void>;
}) {
  const scopeColor = announcement.scope === 'school' ? badgeColorClasses.brand : badgeColorClasses.violet;
  const scopeLabel = announcement.scope === 'school' ? 'Colegio' : className ?? 'Clase';
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
        <div className="flex items-start justify-between gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <span className={`rounded-full px-2.5 py-0.5 text-[11px] font-bold ${scopeColor.bg} ${scopeColor.text}`}>
              {scopeLabel}
            </span>
            {announcement.priority === 'high' && (
              <span className="inline-flex items-center gap-1 rounded-full bg-rose-100 px-2 py-0.5 text-[11px] font-bold text-rose-700 dark:bg-rose-900/40 dark:text-rose-300">
                <AlertCircle className="h-3 w-3" />
                Urgente
              </span>
            )}
          </div>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <h2 className="mt-3 font-display text-xl font-extrabold text-slate-900 dark:text-white">{announcement.title}</h2>
        <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
          {[
            announcement.authorName,
            roleLabel(announcement.authorRole),
            formatRelativeDate(announcement.createdAt),
            announcement.updatedAt && `editado ${formatRelativeDate(announcement.updatedAt)}`,
          ]
            .filter(Boolean)
            .join(' · ')}
        </p>

        <p className="mt-4 whitespace-pre-line text-sm leading-relaxed text-slate-700 dark:text-slate-300">
          {announcement.body}
        </p>

        {announcement.relatedEvent && (
          <Link
            to={`/calendar?event=${announcement.relatedEvent.id}`}
            className="mt-5 inline-flex items-center gap-1.5 rounded-xl border border-violet-200 px-3 py-2 text-sm font-semibold text-violet-700 transition-colors hover:bg-violet-50 dark:border-violet-800 dark:text-violet-300 dark:hover:bg-violet-900/20"
          >
            <ArrowUpRight className="h-4 w-4" />
            Ver evento: {announcement.relatedEvent.title}
          </Link>
        )}

        {announcement.isAuthor && (
          <AuthorActions
            deleteTitle="¿Eliminar este aviso?"
            deleteMessage="Se borrará para todas las familias y no se puede deshacer."
            onEdit={onEdit}
            onDelete={onDelete}
          />
        )}
      </div>
    </div>
  );
}

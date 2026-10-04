import { AlertCircle, ArrowUpRight, Circle } from 'lucide-react';
import { badgeColorClasses } from '@/lib/colorClasses';
import { formatRelativeDate } from '@/lib/date';
import { roleLabel } from '@/lib/roles';
import type { Announcement } from '@/modules/announcements/domain/Announcement';

const priorityStyle: Record<Announcement['priority'], string> = {
  high: 'ring-1 ring-rose-200 dark:ring-rose-900/50',
  normal: '',
  low: 'opacity-75',
};

export function AnnouncementCard({
  announcement,
  className,
  onOpen,
}: {
  announcement: Announcement;
  className?: string;
  onOpen: () => void;
}) {
  const scopeColor = announcement.scope === 'school' ? badgeColorClasses.brand : badgeColorClasses.violet;
  const scopeLabel = announcement.scope === 'school' ? 'Colegio' : className ?? 'Clase';

  return (
    <div
      className={`relative w-full rounded-2xl border border-slate-200 bg-white shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md dark:border-slate-700 dark:bg-slate-900 ${priorityStyle[announcement.priority]}`}
    >
      {!announcement.read && (
        <span role="img" aria-label="No leído" title="No leído" className="absolute right-4 top-4 z-10">
          <Circle className="h-4 w-4 fill-brand-500 text-brand-500" />
        </span>
      )}

      <button type="button" onClick={onOpen} className="w-full p-4 pr-11 text-left">
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

        <h3 className="mt-2.5 font-display text-base font-bold text-slate-900 dark:text-white">{announcement.title}</h3>
        <p className="mt-1 line-clamp-2 text-sm leading-snug text-slate-600 dark:text-slate-400">{announcement.body}</p>

        <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-xs text-slate-500 dark:text-slate-400">
          <span>
            {[announcement.authorName, roleLabel(announcement.authorRole)].filter(Boolean).join(' · ')}
          </span>
          <span>{formatRelativeDate(announcement.createdAt)}</span>
        </div>

        {announcement.relatedEvent && (
          <span className="mt-3 inline-flex items-center gap-1 text-xs font-semibold text-violet-600 dark:text-violet-300">
            <ArrowUpRight className="h-3.5 w-3.5" />
            Ver evento: {announcement.relatedEvent.title}
          </span>
        )}
      </button>
    </div>
  );
}

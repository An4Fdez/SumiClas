import { useMemo, useState } from 'react';
import { AlertCircle, Bell, Plus } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { AnnouncementCard } from '@/components/app/AnnouncementCard';
import { AnnouncementDetailModal } from '@/components/app/AnnouncementDetailModal';
import { AnnouncementForm } from '@/components/app/AnnouncementForm';
import { useAnnouncementRepository } from '@/hooks/useAnnouncementRepository';
import { useChildContext } from '@/hooks/useChildContext';
import { useClassAnnouncements } from '@/hooks/useClassAnnouncements';
import { useClassCalendarEvents } from '@/hooks/useClassCalendarEvents';
import { useWizardIds } from '@/hooks/useWizardIds';
import { getApiErrorStatus } from '@/lib/apiError';
import { badgeColorClasses } from '@/lib/colorClasses';
import { nextOccurrence } from '@/lib/date';
import type { Announcement } from '@/modules/announcements/domain/Announcement';
import type { Priority } from '@/modules/announcements/domain/Priority';

type ScopeFilter = 'all' | 'school' | 'class';
type ReadFilter = 'unread' | 'read' | null;

const scopeFilters: { value: ScopeFilter; label: string }[] = [
  { value: 'all', label: 'Todos' },
  { value: 'class', label: 'Mi clase' },
];

export default function Announcements() {
  const { currentChild, currentRole } = useChildContext();
  const classId = currentChild?.classId;
  const repository = useAnnouncementRepository();
  const ids = useWizardIds();
  const { state, reload, loadMore, loadingMore, loadMoreError, markRead, readError } = useClassAnnouncements(classId);
  const { state: calendarState } = useClassCalendarEvents(classId);
  const relatedEventOptions = useMemo(() => {
    if (calendarState.status !== 'ready') return undefined;
    const today = new Date();
    return calendarState.events
      .filter((ev) => ev.type !== 'birthday' && nextOccurrence(ev, today) !== undefined)
      .map((ev) => ({ id: ev.id, title: ev.title }));
  }, [calendarState]);
  const [scopeFilter, setScopeFilter] = useState<ScopeFilter>('all');
  const [onlyUrgent, setOnlyUrgent] = useState(false);
  const [readFilter, setReadFilter] = useState<ReadFilter>(null);
  const [openAnnouncement, setOpenAnnouncement] = useState<Announcement | null>(null);
  const [formOpen, setFormOpen] = useState(false);

  const announcements = useMemo(
    () =>
      (state.status === 'ready' ? state.items : [])
        .filter((a) => scopeFilter === 'all' || a.scope === scopeFilter)
        .filter((a) => (onlyUrgent ? a.priority === 'high' : true))
        .filter((a) => {
          if (readFilter === 'unread') return !a.read;
          if (readFilter === 'read') return a.read;
          return true;
        }),
    [state, scopeFilter, onlyUrgent, readFilter],
  );

  const canCreate = currentRole === 'classRepresentative' && !!classId;

  const handleOpen = (announcement: Announcement) => {
    setOpenAnnouncement(announcement);
    if (!announcement.read) markRead(announcement.id);
  };

  const handleCreateAnnouncement = async (data: {
    title: string;
    body: string;
    priority: Priority;
    eventId: string | null;
  }) => {
    if (!classId) return;
    const id = ids.take('announcement');
    try {
      await repository.create({
        id,
        scope: 'class',
        classId,
        title: data.title,
        body: data.body,
        priority: data.priority,
        relatedEventId: data.eventId ?? undefined,
      });
    } catch (error) {
      const status = getApiErrorStatus(error);
      if (status !== undefined && status < 500) ids.consume('announcement');
      throw error;
    }
    ids.consume('announcement');
    setFormOpen(false);
    reload();
  };

  return (
    <div>
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div
            className={`grid h-12 w-12 place-items-center rounded-xl ${badgeColorClasses.rose.bg} ${badgeColorClasses.rose.text}`}
          >
            <Bell className="h-6 w-6" />
          </div>
          <h1 className="mt-4 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
            Avisos
          </h1>
          <p className="mt-2 truncate text-slate-600 dark:text-slate-400">
            Los avisos del colegio y de tu clase, del más reciente al más antiguo.
          </p>
        </div>

        {canCreate && (
          <button
            type="button"
            onClick={() => setFormOpen(true)}
            className="inline-flex shrink-0 items-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-3.5 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25"
          >
            <Plus className="h-4 w-4" />
            Nuevo
          </button>
        )}
      </div>

      <div className="mt-6 flex flex-wrap items-center justify-between gap-2">
        <div className="flex flex-wrap items-center gap-2">
          {scopeFilters.map((op) => (
            <button
              key={op.value}
              type="button"
              onClick={() => setScopeFilter(op.value)}
              className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
                scopeFilter === op.value
                  ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
                  : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
              }`}
            >
              {op.label}
            </button>
          ))}

          <button
            type="button"
            onClick={() => setOnlyUrgent((v) => !v)}
            className={`inline-flex items-center gap-1.5 rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
              onlyUrgent
                ? 'bg-rose-100 text-rose-700 dark:bg-rose-900/40 dark:text-rose-300'
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
            }`}
          >
            <AlertCircle className="h-3.5 w-3.5" />
            Urgentes
          </button>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            onClick={() => setReadFilter((v) => (v === 'unread' ? null : 'unread'))}
            className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
              readFilter === 'unread'
                ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
            }`}
          >
            No leídos
          </button>

          <button
            type="button"
            onClick={() => setReadFilter((v) => (v === 'read' ? null : 'read'))}
            className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
              readFilter === 'read'
                ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
            }`}
          >
            Leídos
          </button>
        </div>
      </div>

      {readError && (
        <p role="alert" className="mt-4 text-sm font-semibold text-rose-600 dark:text-rose-400">
          {readError}
        </p>
      )}

      {!classId ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          Tu hijo/a todavía no está en ninguna clase, así que no hay avisos que mostrar.
        </div>
      ) : state.status === 'loading' || state.status === 'idle' ? (
        <p className="mt-8 text-sm text-slate-500 dark:text-slate-400">Cargando los avisos…</p>
      ) : state.status === 'error' ? (
        <div role="alert" className="mt-8 rounded-2xl border border-rose-200 bg-rose-50/60 p-4 dark:border-rose-700/40 dark:bg-rose-900/15">
          <p className="text-sm text-slate-700 dark:text-slate-300">{state.message}</p>
          <button
            type="button"
            onClick={reload}
            className="mt-3 text-sm font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
          >
            Reintentar
          </button>
        </div>
      ) : announcements.length === 0 ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          No hay avisos por ahora.
        </div>
      ) : (
        <div className="mt-6 space-y-3">
          {announcements.map((announcement) => (
            <Reveal key={announcement.id}>
              <AnnouncementCard
                announcement={announcement}
                className={announcement.classId ? currentChild?.className : undefined}
                onOpen={() => handleOpen(announcement)}
              />
            </Reveal>
          ))}
        </div>
      )}

      {state.status === 'ready' && state.nextCursor && (
        <div className="mt-6 text-center">
          <button
            type="button"
            onClick={loadMore}
            disabled={loadingMore}
            className="rounded-full bg-slate-100 px-5 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-200 disabled:opacity-60 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700"
          >
            {loadingMore ? 'Cargando…' : 'Cargar más'}
          </button>
          {loadMoreError && (
            <p role="alert" className="mt-2 text-xs font-semibold text-rose-600 dark:text-rose-400">
              {loadMoreError}
            </p>
          )}
        </div>
      )}

      {openAnnouncement && (
        <AnnouncementDetailModal
          announcement={openAnnouncement}
          className={openAnnouncement.classId ? currentChild?.className : undefined}
          onClose={() => setOpenAnnouncement(null)}
        />
      )}

      {formOpen && currentChild && (
        <AnnouncementForm
          className={currentChild.className}
          events={relatedEventOptions}
          onSubmit={handleCreateAnnouncement}
          onClose={() => setFormOpen(false)}
        />
      )}
    </div>
  );
}

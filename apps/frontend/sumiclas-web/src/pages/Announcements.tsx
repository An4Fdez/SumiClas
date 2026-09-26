import { useMemo, useState } from 'react';
import { AlertCircle, Bell, Plus } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { AnnouncementCard } from '@/components/app/AnnouncementCard';
import { AnnouncementDetailModal } from '@/components/app/AnnouncementDetailModal';
import { AnnouncementForm } from '@/components/app/AnnouncementForm';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses } from '@/lib/colorClasses';
import { getGuardianInfo } from '@/lib/auth';
import { roleMeta } from '@/lib/roles';
import { classAnnouncementsByChild, schoolAnnouncementsMock, type Announcement, type Priority } from '@/data/announcementsMock';
import { calendarEventsMock } from '@/data/calendarMock';

type ScopeFilter = 'all' | 'school' | 'class';
type ReadFilter = 'unread' | 'read' | null;

const scopeFilters: { value: ScopeFilter; label: string }[] = [
  { value: 'all', label: 'Todos' },
  { value: 'school', label: 'Colegio' },
  { value: 'class', label: 'Mi clase' },
];

const priorityWeight: Record<Priority, number> = { high: 0, normal: 1, low: 2 };

export default function Announcements() {
  const { children, currentChild, currentRole } = useChildContext();
  const [createdAnnouncements, setCreatedAnnouncements] = useState<Announcement[]>([]);
  const [read, setRead] = useState<Set<string>>(new Set());
  const [scopeFilter, setScopeFilter] = useState<ScopeFilter>('all');
  const [onlyUrgent, setOnlyUrgent] = useState(false);
  const [readFilter, setReadFilter] = useState<ReadFilter>(null);
  const [openAnnouncement, setOpenAnnouncement] = useState<Announcement | null>(null);
  const [formOpen, setFormOpen] = useState(false);

  const classNameByChild = useMemo(
    () => Object.fromEntries(children.map((c) => [c.id, c.className])),
    [children]
  );

  const allClassAnnouncements = useMemo(
    () => [...children.flatMap((c) => classAnnouncementsByChild[c.id] ?? []), ...createdAnnouncements],
    [children, createdAnnouncements]
  );

  const isRead = (announcement: Announcement) => (read.has(announcement.id) ? !announcement.read : announcement.read);

  const announcements = useMemo(() => {
    const combined =
      scopeFilter === 'school'
        ? schoolAnnouncementsMock
        : scopeFilter === 'class'
          ? allClassAnnouncements
          : [...schoolAnnouncementsMock, ...allClassAnnouncements];

    const filtered = combined
      .filter((a) => (onlyUrgent ? a.priority === 'high' : true))
      .filter((a) => {
        if (readFilter === 'unread') return !isRead(a);
        if (readFilter === 'read') return isRead(a);
        return true;
      });

    return [...filtered].sort((a, b) => {
      if (priorityWeight[a.priority] !== priorityWeight[b.priority]) {
        return priorityWeight[a.priority] - priorityWeight[b.priority];
      }
      return new Date(b.date).getTime() - new Date(a.date).getTime();
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [scopeFilter, onlyUrgent, readFilter, read, allClassAnnouncements]);

  const canCreate = currentRole === 'classRepresentative' && !!currentChild;

  const toggleRead = (id: string) =>
    setRead((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });

  const handleCreateAnnouncement = (data: {
    title: string;
    body: string;
    priority: Priority;
    eventId: string | null;
  }) => {
    if (!currentChild) return;
    const event = calendarEventsMock.find((ev) => ev.id === data.eventId);
    const created: Announcement = {
      id: `local-${Date.now()}`,
      scope: 'class',
      childId: currentChild.id,
      title: data.title,
      body: data.body,
      priority: data.priority,
      authorName: getGuardianInfo()?.name ?? 'Tú',
      authorRole: roleMeta.classRepresentative.label,
      date: new Date().toISOString(),
      relatedEvent: event,
      read: true,
    };
    setCreatedAnnouncements((prev) => [created, ...prev]);
    setFormOpen(false);
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
            Los avisos del colegio y de tu clase, ordenados por fecha y con lo prioritario destacado arriba.
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

      {announcements.length === 0 ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          No hay avisos por ahora.
        </div>
      ) : (
        <div className="mt-6 space-y-3">
          {announcements.map((announcement) => (
            <Reveal key={announcement.id}>
              <AnnouncementCard
                announcement={{ ...announcement, read: isRead(announcement) }}
                className={announcement.childId ? classNameByChild[announcement.childId] : undefined}
                onOpen={() => setOpenAnnouncement(announcement)}
                onToggleRead={() => toggleRead(announcement.id)}
              />
            </Reveal>
          ))}
        </div>
      )}

      {openAnnouncement && (
        <AnnouncementDetailModal
          announcement={openAnnouncement}
          className={openAnnouncement.childId ? classNameByChild[openAnnouncement.childId] : undefined}
          onClose={() => setOpenAnnouncement(null)}
        />
      )}

      {formOpen && currentChild && (
        <AnnouncementForm
          className={currentChild.className}
          events={calendarEventsMock}
          onSubmit={handleCreateAnnouncement}
          onClose={() => setFormOpen(false)}
        />
      )}
    </div>
  );
}

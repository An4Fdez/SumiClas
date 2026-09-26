import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Calendar as CalendarIcon, Plus, Repeat } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { CalendarGrid } from '@/components/app/CalendarGrid';
import { CalendarWeekGrid } from '@/components/app/CalendarWeekGrid';
import { CalendarNavHeader } from '@/components/app/CalendarNavHeader';
import { CalendarDayAgenda } from '@/components/app/CalendarDayAgenda';
import { DayDetailModal } from '@/components/app/DayDetailModal';
import { EventDetailModal } from '@/components/app/EventDetailModal';
import { EventAgendaRow } from '@/components/app/EventAgendaRow';
import { EventForm, type NewEvent } from '@/components/app/EventForm';
import { EventTypeFilterDropdown } from '@/components/app/EventTypeFilterDropdown';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses } from '@/lib/colorClasses';
import { getGuardianInfo } from '@/lib/auth';
import { roleMeta } from '@/lib/roles';
import { visibleYearForEvent, isSameCalendarDay, eventOccursOnDay, formatFullDay } from '@/lib/date';
import { calendarEventsMock, type CalendarEvent, type EventType } from '@/data/calendarMock';

type ViewMode = 'day' | 'week' | 'month' | 'list';
type ScopeFilter = 'all' | 'school' | 'class';

const viewOrder: ViewMode[] = ['day', 'week', 'month', 'list'];
const viewLabels: Record<ViewMode, string> = { day: 'Día', week: 'Semana', month: 'Mes', list: 'Lista' };

const scopeFilters: { value: ScopeFilter; label: string }[] = [
  { value: 'all', label: 'Todos' },
  { value: 'school', label: 'Colegio' },
  { value: 'class', label: 'Mi clase' },
];

export default function Calendar() {
  const { children, currentChild, currentRole } = useChildContext();
  const [searchParams, setSearchParams] = useSearchParams();

  const [view, setView] = useState<ViewMode>('month');
  const [referenceDate, setReferenceDate] = useState(new Date());
  const [scopeFilter, setScopeFilter] = useState<ScopeFilter>('all');
  const [typeFilters, setTypeFilters] = useState<Set<EventType>>(new Set());
  const [openDayModal, setOpenDayModal] = useState<Date | null>(null);
  const [openEvent, setOpenEvent] = useState<CalendarEvent | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [createdEvents, setCreatedEvents] = useState<CalendarEvent[]>([]);

  const classNameByChild = useMemo(
    () => Object.fromEntries(children.map((c) => [c.id, c.className])),
    [children]
  );

  const allEvents = useMemo(
    () => [...calendarEventsMock, ...createdEvents],
    [createdEvents]
  );

  const visibleEvents = useMemo(() => {
    const myChildIds = new Set(children.map((c) => c.id));
    return allEvents
      .filter((ev) => {
        if (scopeFilter === 'school') return ev.scope === 'school';
        if (scopeFilter === 'class') return ev.scope === 'class' && !!ev.childId && myChildIds.has(ev.childId);
        if (ev.scope === 'school') return true;
        return !!ev.childId && myChildIds.has(ev.childId);
      })
      .filter((ev) => (typeFilters.size === 0 ? true : typeFilters.has(ev.type)));
  }, [allEvents, scopeFilter, typeFilters, children]);

  const eventsForDay = (day: Date): CalendarEvent[] =>
    visibleEvents.filter((ev) => eventOccursOnDay(ev.date, day, ev.type === 'birthday'));

  const classNameForEvent = (event: CalendarEvent) =>
    event.childId ? classNameByChild[event.childId] : undefined;

  const listGroups = useMemo(() => {
    const todayStart = new Date();
    todayStart.setHours(0, 0, 0, 0);

    const withEffectiveDate = visibleEvents.map((ev) => {
      if (ev.type === 'birthday') {
        const base = new Date(ev.date);
        let candidate = new Date(todayStart.getFullYear(), base.getMonth(), base.getDate());
        if (candidate < todayStart) candidate = new Date(todayStart.getFullYear() + 1, base.getMonth(), base.getDate());
        return { event: ev, effectiveDate: candidate };
      }
      return { event: ev, effectiveDate: new Date(ev.date) };
    });

    const upcoming = withEffectiveDate
      .filter(({ effectiveDate }) => effectiveDate >= todayStart)
      .sort((a, b) => a.effectiveDate.getTime() - b.effectiveDate.getTime());

    const groups: { date: Date; events: CalendarEvent[] }[] = [];
    for (const { event, effectiveDate } of upcoming) {
      const group = groups.find((g) => isSameCalendarDay(g.date, effectiveDate));
      if (group) group.events.push(event);
      else groups.push({ date: effectiveDate, events: [event] });
    }
    return groups;
  }, [visibleEvents]);

  useEffect(() => {
    const eventId = searchParams.get('event');
    if (!eventId) return;
    const event = allEvents.find((ev) => ev.id === eventId);
    if (!event) return;

    const eventDate = new Date(event.date);
    const targetYear = visibleYearForEvent(event.date, event.type === 'birthday');

    setView('month');
    setReferenceDate(new Date(targetYear, eventDate.getMonth(), eventDate.getDate()));
    setOpenEvent(event);
    setSearchParams({}, { replace: true });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchParams]);

  const changeDate = (delta: number) => {
    setReferenceDate((prev) => {
      const next = new Date(prev);
      if (view === 'day') next.setDate(prev.getDate() + delta);
      else if (view === 'week') next.setDate(prev.getDate() + delta * 7);
      else next.setMonth(prev.getMonth() + delta);
      return next;
    });
  };

  const goToToday = () => setReferenceDate(new Date());

  const nextView = () =>
    setView((v) => viewOrder[(viewOrder.indexOf(v) + 1) % viewOrder.length]);

  const canPublishOfficial = currentRole === 'classRepresentative';
  const canCreate = !!currentChild;

  const handleCreateEvent = (data: NewEvent) => {
    if (!currentChild) return;

    let createdEvent: CalendarEvent;
    if (data.type === 'birthday') {
      const child = children.find((c) => c.id === data.childId) ?? currentChild;
      createdEvent = {
        id: `local-${Date.now()}`,
        scope: 'class',
        childId: child.id,
        type: 'birthday',
        title: `Cumpleaños de ${child.studentName}`,
        date: new Date(1900, data.month - 1, data.day).toISOString(),
        authorName: getGuardianInfo()?.name ?? 'Tú',
        authorRole: 'Familia',
      };
    } else if (data.type === 'other') {
      const [y, m, d] = data.date.split('-').map(Number);
      const [h, min] = data.time ? data.time.split(':').map(Number) : [0, 0];
      const eventDate = new Date(y, m - 1, d, h, min);
      createdEvent = {
        id: `local-${Date.now()}`,
        scope: data.scope,
        childId: data.scope === 'class' ? currentChild.id : undefined,
        type: 'other',
        title: data.title,
        date: eventDate.toISOString(),
        time: data.time || undefined,
        location: data.location || undefined,
        cost: data.cost || undefined,
        authorName: getGuardianInfo()?.name ?? 'Tú',
        authorRole: roleMeta.classRepresentative.label,
      };
    } else {
      const [y, m, d] = data.date.split('-').map(Number);
      const [h, min] = data.time ? data.time.split(':').map(Number) : [0, 0];
      const eventDate = new Date(y, m - 1, d, h, min);
      createdEvent = {
        id: `local-${Date.now()}`,
        scope: 'class',
        childId: currentChild.id,
        type: data.type,
        title: data.title,
        date: eventDate.toISOString(),
        time: data.time || undefined,
        location: data.location || undefined,
        cost: data.cost || undefined,
        authorName: getGuardianInfo()?.name ?? 'Tú',
        authorRole: roleMeta.classRepresentative.label,
      };
    }

    setCreatedEvents((prev) => [...prev, createdEvent]);
    setFormOpen(false);

    const targetYear = visibleYearForEvent(createdEvent.date, createdEvent.type === 'birthday');
    const newDate = new Date(createdEvent.date);
    setView('month');
    setReferenceDate(new Date(targetYear, newDate.getMonth(), newDate.getDate()));
  };

  return (
    <div>
      <div className="flex items-start justify-between gap-3">
        <div>
          <div
            className={`grid h-12 w-12 place-items-center rounded-xl ${badgeColorClasses.violet.bg} ${badgeColorClasses.violet.text}`}
          >
            <CalendarIcon className="h-6 w-6" />
          </div>
          <h1 className="mt-4 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
            Calendario
          </h1>
          <p className="mt-2 max-w-xl text-slate-600 dark:text-slate-400">
            Excursiones, reuniones y actividades de la clase, todo en un mismo sitio.
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

          <span className="mx-0.5 h-5 w-px bg-slate-200 dark:bg-slate-700" aria-hidden="true" />

          <EventTypeFilterDropdown selected={typeFilters} onChange={setTypeFilters} />
        </div>

        <button
          type="button"
          onClick={nextView}
          className="inline-flex items-center gap-1.5 rounded-full bg-brand-600 px-3.5 py-1.5 text-sm font-semibold text-white transition-colors hover:bg-brand-700"
        >
          <Repeat className="h-3.5 w-3.5" />
          {viewLabels[view]}
        </button>
      </div>

      {view === 'month' && (
        <Reveal className="mt-6">
          <CalendarGrid
            year={referenceDate.getFullYear()}
            month={referenceDate.getMonth()}
            eventsForDay={eventsForDay}
            onChangeMonth={changeDate}
            onToday={goToToday}
            onOpenDay={setOpenDayModal}
          />
        </Reveal>
      )}

      {view === 'week' && (
        <Reveal className="mt-6">
          <CalendarWeekGrid
            referenceDate={referenceDate}
            eventsForDay={eventsForDay}
            onChangeWeek={changeDate}
            onToday={goToToday}
            onOpenDay={setOpenDayModal}
          />
        </Reveal>
      )}

      {view === 'day' && (
        <Reveal className="mt-6">
          <div className="rounded-2xl border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900">
            <CalendarNavHeader
              title={formatFullDay(referenceDate)}
              onPrev={() => changeDate(-1)}
              onNext={() => changeDate(1)}
              onToday={goToToday}
            />
            <CalendarDayAgenda
              day={referenceDate}
              events={eventsForDay(referenceDate)}
              classNameForEvent={classNameForEvent}
              onOpenEvent={setOpenEvent}
              showDate={false}
            />
          </div>
        </Reveal>
      )}

      {view === 'list' && (
        <div className="mt-6">
          {listGroups.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
              No hay eventos próximos.
            </div>
          ) : (
            <div className="space-y-6">
              {listGroups.map((group) => (
                <Reveal key={group.date.toISOString()}>
                  <h3 className="text-sm font-bold text-slate-500 dark:text-slate-400">
                    {formatFullDay(group.date)}
                  </h3>
                  <div className="mt-2 space-y-2">
                    {group.events.map((event) => (
                      <EventAgendaRow
                        key={event.id}
                        event={event}
                        scopeLabel={event.scope === 'school' ? 'Colegio' : (classNameForEvent(event) ?? 'Clase')}
                        onOpen={() => setOpenEvent(event)}
                      />
                    ))}
                  </div>
                </Reveal>
              ))}
            </div>
          )}
        </div>
      )}

      {openDayModal && (
        <DayDetailModal
          day={openDayModal}
          events={eventsForDay(openDayModal)}
          classNameForEvent={classNameForEvent}
          onOpenEvent={(event) => {
            setOpenDayModal(null);
            setOpenEvent(event);
          }}
          onClose={() => setOpenDayModal(null)}
        />
      )}

      {openEvent && (
        <EventDetailModal
          event={openEvent}
          className={classNameForEvent(openEvent)}
          onClose={() => setOpenEvent(null)}
        />
      )}

      {formOpen && currentChild && (
        <EventForm
          canPublishOfficial={canPublishOfficial}
          className={currentChild.className}
          availableChildren={children}
          initialChildId={currentChild.id}
          onSubmit={handleCreateEvent}
          onClose={() => setFormOpen(false)}
        />
      )}
    </div>
  );
}

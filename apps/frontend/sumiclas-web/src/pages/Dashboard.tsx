import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Bell, Calendar, BookOpen, Wallet, PackageSearch, AlertCircle, Sparkles, type LucideIcon } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { WeeklySummaryCard } from '@/components/app/WeeklySummaryCard';
import { useChildContext } from '@/hooks/useChildContext';
import { useClassDashboardSummary } from '@/hooks/useClassDashboardSummary';
import { badgeColorClasses, type PaletteColor } from '@/lib/colorClasses';
import { aiSummaryByChild } from '@/data/appMock';
import { formatDueDate } from '@/lib/date';
import { roleMeta, type ClassRole } from '@/lib/roles';
import type { DashboardSummary } from '@/modules/dashboard/domain/DashboardSummary';
import { DEFAULT_SUMMARY_PERIOD, SUMMARY_PERIODS, type SummaryPeriod } from '@/modules/dashboard/domain/SummaryPeriod';

type OnboardingState = {
  school?: string;
  schoolClass?: string;
  student?: string;
  secondGuardian?: boolean;
  assignedRole?: ClassRole;
};

export default function Dashboard() {
  const location = useLocation();
  const onboardingState = (location.state ?? {}) as OnboardingState;
  const { currentChild, currentChildId } = useChildContext();
  const [period, setPeriod] = useState<SummaryPeriod>(DEFAULT_SUMMARY_PERIOD);
  const { state, reload } = useClassDashboardSummary(currentChild?.classId, period);
  const aiSummary = aiSummaryByChild[currentChildId];

  return (
    <div>
      {onboardingState.student && (
        <Reveal className="mb-6">
          <div className="flex items-start gap-3 rounded-2xl border border-mint-200 bg-mint-50/60 p-4 dark:border-mint-700/40 dark:bg-mint-900/15">
            <div className="grid h-9 w-9 shrink-0 place-items-center rounded-xl bg-gradient-to-br from-brand-400 via-mint-400 to-rose-400 text-white">
              <Sparkles className="h-4 w-4" />
            </div>
            <p className="text-sm text-slate-700 dark:text-slate-300">
              Hemos {onboardingState.secondGuardian ? 'vinculado' : 'dado de alta'} a{' '}
              <strong className="font-semibold text-slate-900 dark:text-white">{onboardingState.student}</strong>
              {onboardingState.school && (
                <>
                  {' '}
                  en <strong className="font-semibold text-slate-900 dark:text-white">{onboardingState.school}</strong>
                </>
              )}
              .{' '}
              {onboardingState.assignedRole && (
                <>
                  Se te ha asignado el rol de{' '}
                  <strong className="font-semibold text-slate-900 dark:text-white">
                    {roleMeta[onboardingState.assignedRole].label}
                  </strong>{' '}
                  de esta clase.{' '}
                </>
              )}
              ¡Bienvenido/a!
            </p>
          </div>
        </Reveal>
      )}

      <Reveal>
        <h1 className="font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
          Esto es lo importante ahora mismo
        </h1>
        <p className="mt-1.5 text-sm text-slate-600 dark:text-slate-400">
          Un vistazo rápido, sin tener que entrar en cada sección.
        </p>
      </Reveal>

      <Reveal delay={20} className="mt-5">
        <PeriodSelector value={period} onChange={setPeriod} />
      </Reveal>

      {aiSummary && (
        <Reveal delay={40} className="mt-6">
          <WeeklySummaryCard date={aiSummary.date} points={aiSummary.points} />
        </Reveal>
      )}

      {!currentChild ? (
        <Reveal delay={80} className="mt-6">
          <div className="rounded-2xl border border-dashed border-slate-300 bg-white p-8 text-center dark:border-slate-700 dark:bg-slate-900">
            <p className="text-sm text-slate-600 dark:text-slate-400">
              Todavía no tienes ningún hijo/a registrado.{' '}
              <Link to="/onboarding" className="font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400">
                Empieza aquí
              </Link>
            </p>
          </div>
        </Reveal>
      ) : state.status === 'idle' ? (
        <p className="mt-8 text-sm text-slate-500 dark:text-slate-400">
          Este alumno todavía no está en ninguna clase, así que no hay novedades que mostrar.
        </p>
      ) : state.status === 'loading' ? (
        <p role="status" className="mt-8 text-sm text-slate-500 dark:text-slate-400">
          Cargando el resumen…
        </p>
      ) : state.status === 'error' ? (
        <div
          role="alert"
          className="mt-8 rounded-2xl border border-rose-200 bg-rose-50/60 p-4 dark:border-rose-700/40 dark:bg-rose-900/15"
        >
          <p className="text-sm text-slate-700 dark:text-slate-300">{state.message}</p>
          <button
            type="button"
            onClick={reload}
            className="mt-3 text-sm font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
          >
            Reintentar
          </button>
        </div>
      ) : (
        <SummaryCards summary={state.summary} />
      )}
    </div>
  );
}

const PERIOD_LABEL = 'Novedades de los últimos';

function PeriodSelector({ value, onChange }: { value: SummaryPeriod; onChange: (period: SummaryPeriod) => void }) {
  return (
    <div role="radiogroup" aria-label={PERIOD_LABEL} className="flex flex-wrap items-center gap-2">
      <span className="text-sm text-slate-600 dark:text-slate-400">{PERIOD_LABEL}</span>
      {SUMMARY_PERIODS.map((days) => (
        <button
          key={days}
          type="button"
          role="radio"
          aria-checked={days === value}
          onClick={() => onChange(days)}
          className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
            days === value
              ? 'bg-brand-600 text-white'
              : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
          }`}
        >
          {days} días
        </button>
      ))}
    </div>
  );
}

function withCount(count: number, singular: string, pluralForm: string): string {
  return `${count} ${count === 1 ? singular : pluralForm}`;
}

function expenseLabel(expenses: DashboardSummary['expenses']): string {
  if (!expenses.pending) return 'Todo al día';
  return expenses.item ? `Pago pendiente: ${expenses.item}` : 'Pago pendiente';
}

function whenLabel(daysUntil: number): string {
  if (daysUntil === 0) return 'hoy';
  if (daysUntil === 1) return 'mañana';
  return `en ${daysUntil} días`;
}

function SummaryCards({ summary }: { summary: DashboardSummary }) {
  const nextEvent = summary.upcomingEvents[0];
  const moreEvents = summary.upcomingEvents.length - 1;

  return (
    <>
      <div className="mt-10 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <Reveal delay={0}>
          <SectionCard
            to="/announcements"
            color="rose"
            icon={Bell}
            title="Avisos"
            value={summary.announcements.unread > 0 ? `${summary.announcements.unread} sin leer` : 'Sin novedades'}
            highlight={summary.announcements.urgent > 0}
            highlightLabel={withCount(summary.announcements.urgent, 'urgente', 'urgentes')}
          />
        </Reveal>

        <Reveal delay={60}>
          <SectionCard
            to="/calendar"
            color="violet"
            icon={Calendar}
            title="Calendario"
            value={
              nextEvent
                ? `${nextEvent.title} ${whenLabel(nextEvent.daysUntil)}${moreEvents > 0 ? ` y ${moreEvents} más` : ''}`
                : 'Sin eventos próximos'
            }
          />
        </Reveal>

        <Reveal delay={120}>
          <SectionCard
            to="/homework"
            color="mint"
            icon={BookOpen}
            title="Deberes"
            value={
              summary.homework.pending > 0 && summary.homework.nextDueDate
                ? `${withCount(summary.homework.pending, 'pendiente', 'pendientes')} · próxima entrega ${formatDueDate(
                    summary.homework.nextDueDate,
                  ).toLowerCase()}`
                : 'Sin deberes pendientes'
            }
          />
        </Reveal>
      </div>

      <div className="mt-4 flex flex-wrap justify-center gap-4">
        <Reveal delay={180} className="w-full sm:w-[calc(50%-0.5rem)] lg:w-[calc(33.333%-0.667rem)]">
          <SectionCard
            to="/expenses"
            color="sun"
            icon={Wallet}
            title="Gastos de clase"
            value={expenseLabel(summary.expenses)}
            highlight={summary.expenses.pending}
            highlightLabel="Pendiente"
          />
        </Reveal>

        <Reveal delay={240} className="w-full sm:w-[calc(50%-0.5rem)] lg:w-[calc(33.333%-0.667rem)]">
          <SectionCard
            to="/lost-and-found"
            color="orange"
            icon={PackageSearch}
            title="Objetos perdidos"
            value={
              summary.lostAndFound.count > 0
                ? withCount(summary.lostAndFound.count, 'objeto sin recuperar', 'objetos sin recuperar')
                : 'Sin novedades'
            }
          />
        </Reveal>
      </div>
    </>
  );
}

function SectionCard({
  to,
  color,
  icon: Icon,
  title,
  value,
  highlight = false,
  highlightLabel,
}: {
  to: string;
  color: PaletteColor;
  icon: LucideIcon;
  title: string;
  value: string;
  highlight?: boolean;
  highlightLabel?: string;
}) {
  const c = badgeColorClasses[color];
  return (
    <Link
      to={to}
      className="group flex h-full flex-col rounded-2xl border border-slate-200 bg-white p-5 shadow-sm transition-all duration-300 hover:-translate-y-1.5 hover:shadow-xl dark:border-slate-700 dark:bg-slate-900"
    >
      <div className="flex items-center justify-between">
        <div
          className={`grid h-11 w-11 place-items-center rounded-xl ${c.bg} ${c.text} transition-transform duration-300 group-hover:scale-110`}
        >
          <Icon className="h-5 w-5" strokeWidth={2} />
        </div>
        {highlight && (
          <span className="inline-flex items-center gap-1 rounded-full bg-rose-100 px-2 py-0.5 text-[11px] font-bold text-rose-700 dark:bg-rose-900/40 dark:text-rose-300">
            <AlertCircle className="h-3 w-3" />
            {highlightLabel}
          </span>
        )}
      </div>
      <h3 className="mt-4 font-display text-base font-bold text-slate-900 dark:text-white">{title}</h3>
      <p className="mt-1 text-sm leading-snug text-slate-600 dark:text-slate-400">{value}</p>
    </Link>
  );
}

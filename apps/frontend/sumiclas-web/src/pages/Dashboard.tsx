import { Link, useLocation } from 'react-router-dom';
import { Bell, Calendar, BookOpen, Wallet, PackageSearch, AlertCircle, Sparkles, type LucideIcon } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { WeeklySummaryCard } from '@/components/app/WeeklySummaryCard';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses, type PaletteColor } from '@/lib/colorClasses';
import { aiSummaryByChild } from '@/data/appMock';
import { roleMeta, type ClassRole } from '@/lib/roles';

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
  const { currentChildId, currentSummary } = useChildContext();
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
          Esto es lo importante de esta semana
        </h1>
        <p className="mt-1.5 text-sm text-slate-600 dark:text-slate-400">
          Un vistazo rápido, sin tener que entrar en cada sección.
        </p>
      </Reveal>

      {currentSummary && aiSummary && (
        <Reveal delay={40} className="mt-6">
          <WeeklySummaryCard date={aiSummary.date} points={aiSummary.points} />
        </Reveal>
      )}

      {!currentSummary ? (
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
      ) : (
        <div className="mt-16 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <Reveal delay={0}>
            <SectionCard
              to="/announcements"
              color="rose"
              icon={Bell}
              title="Avisos"
              value={
                currentSummary.announcements.new > 0
                  ? `${currentSummary.announcements.new} ${currentSummary.announcements.new === 1 ? 'nuevo' : 'nuevos'}`
                  : 'Sin novedades'
              }
              highlight={currentSummary.announcements.urgent}
              highlightLabel="Urgente"
            />
          </Reveal>

          <Reveal delay={60}>
            <SectionCard
              to="/calendar"
              color="violet"
              icon={Calendar}
              title="Calendario"
              value={
                currentSummary.calendar.next
                  ? `${currentSummary.calendar.next} en ${currentSummary.calendar.daysUntil} ${
                      currentSummary.calendar.daysUntil === 1 ? 'día' : 'días'
                    }`
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
                currentSummary.homework.pending > 0
                  ? `${currentSummary.homework.pending} ${
                      currentSummary.homework.pending === 1 ? 'pendiente' : 'pendientes'
                    } para ${currentSummary.homework.nextDueDate}`
                  : 'Sin deberes pendientes'
              }
            />
          </Reveal>
        </div>
      )}

      {currentSummary && (
        <div className="mt-4 flex flex-wrap justify-center gap-4">
          <Reveal delay={180} className="w-full sm:w-[calc(50%-0.5rem)] lg:w-[calc(33.333%-0.667rem)]">
            <SectionCard
              to="/expenses"
              color="sun"
              icon={Wallet}
              title="Gastos de clase"
              value={
                currentSummary.expenses.pending
                  ? `Pago pendiente: ${currentSummary.expenses.item}`
                  : 'Todo al día'
              }
              highlight={currentSummary.expenses.pending}
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
                currentSummary.lostAndFound.hasUpdates
                  ? `${currentSummary.lostAndFound.count} ${
                      currentSummary.lostAndFound.count === 1 ? 'novedad' : 'novedades'
                    }`
                  : 'Sin novedades'
              }
            />
          </Reveal>
        </div>
      )}
    </div>
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

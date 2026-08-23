import { Search, ArrowRight, Bell, Calendar, BookOpen, CheckCircle2, Sparkles } from 'lucide-react';
import { Reveal } from './Reveal';

export function Hero() {
  return (
    <section id="top" className="relative overflow-hidden pt-28 pb-20 sm:pt-32 lg:pt-40 lg:pb-28">
      {/* Background decorations */}
      <div className="pointer-events-none absolute inset-0 -z-10">
        <div className="absolute inset-0 grid-pattern opacity-60 dark:opacity-30" />
        <div className="absolute -left-24 top-10 h-72 w-72 rounded-full bg-brand-300/40 blur-3xl animate-blob-1 dark:bg-brand-600/20" />
        <div className="absolute -right-16 top-32 h-80 w-80 rounded-full bg-mint-300/40 blur-3xl animate-blob-2 dark:bg-mint-600/20" />
        <div className="absolute left-1/3 top-1/2 h-64 w-64 rounded-full bg-rose-300/30 blur-3xl animate-blob-1 dark:bg-rose-600/15" />
      </div>

      <div className="mx-auto grid max-w-7xl items-center gap-12 px-4 sm:px-6 lg:grid-cols-2 lg:gap-8 lg:px-8">
        {/* Left — copy */}
        <div className="text-center lg:text-left">
          <Reveal>
            <span className="inline-flex items-center gap-2 rounded-full border border-mint-200 bg-mint-50 px-3 py-1 text-sm font-semibold text-mint-700 dark:border-mint-700/40 dark:bg-mint-900/30 dark:text-mint-300">
              <span className="relative flex h-2 w-2">
                <span className="absolute inline-flex h-full w-full animate-pulse-ring rounded-full bg-mint-400" />
                <span className="relative inline-flex h-2 w-2 rounded-full bg-mint-500" />
              </span>
              Ya disponible para tu colegio
            </span>
          </Reveal>

          <Reveal delay={80}>
            <h1 className="mt-6 font-display text-4xl font-extrabold leading-[1.1] tracking-tight text-slate-900 dark:text-white sm:text-5xl lg:text-6xl">
              Toda la clase,
              <br />
              <span className="text-gradient bg-gradient-to-r from-brand-500 via-mint-500 to-rose-500">
                en un solo sitio
              </span>
            </h1>
          </Reveal>

          <Reveal delay={160}>
            <p className="mx-auto mt-6 max-w-xl text-lg leading-relaxed text-slate-600 dark:text-slate-300 lg:mx-0">
              Avisos, calendario, deberes, gastos y objetos perdidos. Adiós al
              grupo de WhatsApp saturado — y un{' '}
              <strong className="font-semibold text-slate-900 dark:text-white">
                resumen semanal con IA
              </strong>{' '}
              que te lo resume todo en segundos.
            </p>
          </Reveal>

          {/* CTA */}
          <Reveal delay={240}>
            <div id="buscar-colegio" className="mt-8 flex flex-col items-center gap-3 sm:flex-row lg:justify-start">
              <div className="flex w-full max-w-md items-center gap-2 rounded-2xl border border-slate-200 bg-white p-2 shadow-lg shadow-slate-900/5 dark:border-slate-700 dark:bg-slate-900">
                <Search className="ml-2 h-5 w-5 shrink-0 text-slate-400" />
                <input
                  type="text"
                  placeholder="Escribe el nombre de tu colegio…"
                  className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
                />
                <button className="inline-flex shrink-0 items-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-md shadow-brand-500/25 transition-transform hover:scale-[1.03] active:scale-95">
                  Empezar
                  <ArrowRight className="h-4 w-4" />
                </button>
              </div>
            </div>
          </Reveal>

          <Reveal delay={320}>
            <p className="mt-4 text-sm text-slate-500 dark:text-slate-400">
              Sin instalar nada · Solo para padres y tutores
            </p>
          </Reveal>
        </div>

        {/* Right — phone mockup */}
        <Reveal delay={200} className="relative mx-auto w-full max-w-sm lg:max-w-md">
          <PhoneMockup />
        </Reveal>
      </div>
    </section>
  );
}

function PhoneMockup() {
  return (
    <div className="relative">
      {/* glow */}
      <div className="absolute -inset-6 -z-10 rounded-[3rem] bg-gradient-to-br from-brand-400/30 via-mint-400/30 to-rose-400/30 blur-2xl" />

      {/* Floating notification cards */}
      <div className="absolute -left-10 top-16 z-20 hidden animate-float rounded-2xl border border-slate-200 bg-white p-3 shadow-xl dark:border-slate-700 dark:bg-slate-800 sm:block">
        <div className="flex items-center gap-2">
          <div className="grid h-8 w-8 place-items-center rounded-lg bg-rose-100 text-rose-600 dark:bg-rose-900/40 dark:text-rose-300">
            <Bell className="h-4 w-4" />
          </div>
          <div className="text-left">
            <p className="text-xs font-bold text-slate-900 dark:text-white">Aviso prioritario</p>
            <p className="text-[11px] text-slate-500 dark:text-slate-400">Mañana no hay clase</p>
          </div>
        </div>
      </div>

      <div className="absolute -right-8 bottom-24 z-20 hidden animate-float-slow rounded-2xl border border-slate-200 bg-white p-3 shadow-xl dark:border-slate-700 dark:bg-slate-800 sm:block">
        <div className="flex items-center gap-2">
          <div className="grid h-8 w-8 place-items-center rounded-lg bg-brand-100 text-brand-600 dark:bg-brand-900/40 dark:text-brand-300">
            <Sparkles className="h-4 w-4" />
          </div>
          <div className="text-left">
            <p className="text-xs font-bold text-slate-900 dark:text-white">Resumen IA listo</p>
            <p className="text-[11px] text-slate-500 dark:text-slate-400">3 cosas esta semana</p>
          </div>
        </div>
      </div>

      {/* Phone frame */}
      <div className="relative mx-auto w-[280px] rounded-[2.5rem] border-[6px] border-slate-900 bg-slate-900 shadow-2xl dark:border-slate-800 sm:w-[320px]">
        <div className="absolute left-1/2 top-2 z-10 h-5 w-24 -translate-x-1/2 rounded-full bg-slate-900 dark:bg-slate-800" />
        <div className="overflow-hidden rounded-[2rem] bg-gradient-to-b from-slate-50 to-slate-100 dark:from-slate-900 dark:to-slate-950">
          {/* Screen header */}
          <div className="px-4 pb-3 pt-9">
            <div className="flex items-center justify-between">
              <p className="font-display text-sm font-bold text-slate-900 dark:text-white">Avisos</p>
              <span className="rounded-full bg-rose-500 px-2 py-0.5 text-[10px] font-bold text-white">3 nuevos</span>
            </div>
          </div>

          {/* AI summary mini-card */}
          <div className="mx-4 mb-3 rounded-xl bg-gradient-to-br from-brand-500 via-mint-500 to-rose-500 p-[1px]">
            <div className="rounded-[11px] bg-white/95 p-3 backdrop-blur dark:bg-slate-900/90">
              <div className="flex items-center gap-1.5">
                <Sparkles className="h-3.5 w-3.5 text-brand-500" />
                <p className="text-[11px] font-bold text-slate-900 dark:text-white">Resumen de la semana</p>
              </div>
              <p className="mt-1.5 text-[11px] leading-snug text-slate-600 dark:text-slate-300">
                Jueves: excursión al museo (traer autorización). Viernes: examen de mates. Sábado: cumpleaños de Martina.
              </p>
            </div>
          </div>

          {/* Notice cards */}
          <div className="space-y-2 px-4 pb-5">
            <NoticeCard color="rose" icon={<Bell className="h-3.5 w-3.5" />} title="Mañana no hay clase" meta="Toda la clase · Tutora" priority />
            <NoticeCard color="brand" icon={<Calendar className="h-3.5 w-3.5" />} title="Excursión al museo — jueves" meta="Toda la clase · Delegada" />
            <NoticeCard color="mint" icon={<BookOpen className="h-3.5 w-3.5" />} title="Examen de mates — viernes" meta="Toda la clase · Tutora" />
            <NoticeCard color="sun" icon={<CheckCircle2 className="h-3.5 w-3.5" />} title="Regalo profe: 18 de 24 pagados" meta="Toda la clase · Delegada" />
          </div>
        </div>
      </div>
    </div>
  );
}

type NoticeColor = 'rose' | 'brand' | 'mint' | 'sun';

const colorMap: Record<NoticeColor, string> = {
  rose: 'bg-rose-100 text-rose-600 dark:bg-rose-900/40 dark:text-rose-300',
  brand: 'bg-brand-100 text-brand-600 dark:bg-brand-900/40 dark:text-brand-300',
  mint: 'bg-mint-100 text-mint-600 dark:bg-mint-900/40 dark:text-mint-300',
  sun: 'bg-sun-100 text-sun-600 dark:bg-sun-900/40 dark:text-sun-300',
};

function NoticeCard({
  color,
  icon,
  title,
  meta,
  priority = false,
}: {
  color: NoticeColor;
  icon: React.ReactNode;
  title: string;
  meta: string;
  priority?: boolean;
}) {
  return (
    <div
      className={`flex items-start gap-2.5 rounded-xl border p-2.5 ${
        priority
          ? 'border-rose-200 bg-rose-50/80 dark:border-rose-800/50 dark:bg-rose-900/20'
          : 'border-slate-200 bg-white dark:border-slate-700 dark:bg-slate-800/60'
      }`}
    >
      <div className={`grid h-7 w-7 shrink-0 place-items-center rounded-lg ${colorMap[color]}`}>{icon}</div>
      <div className="min-w-0 flex-1">
        <p className="truncate text-[12px] font-semibold text-slate-900 dark:text-white">{title}</p>
        <p className="mt-0.5 text-[10px] text-slate-400 dark:text-slate-500">{meta}</p>
      </div>
      {priority && <span className="mt-0.5 h-2 w-2 shrink-0 rounded-full bg-rose-500" />}
    </div>
  );
}

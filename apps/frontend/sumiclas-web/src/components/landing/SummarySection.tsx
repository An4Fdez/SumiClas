import { Sparkles, Bell, Calendar, BookOpen, CheckCircle2, Gift } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';

export function SummarySection() {
  return (
    <section id="how-it-works" className="relative overflow-hidden py-20 sm:py-28">
      <div className="pointer-events-none absolute inset-0 -z-10">
        <div className="absolute left-1/2 top-1/2 h-96 w-96 -translate-x-1/2 -translate-y-1/2 rounded-full bg-mint-300/20 blur-3xl dark:bg-mint-600/10" />
      </div>

      <div className="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8">
        <div className="grid items-center gap-12 lg:grid-cols-2 lg:gap-16">
          <div>
            <Reveal>
              <span className="inline-flex items-center gap-2 rounded-full border border-brand-200 bg-brand-50 px-3 py-1 text-sm font-semibold text-brand-700 dark:border-brand-700/40 dark:bg-brand-900/30 dark:text-brand-300">
                <Sparkles className="h-4 w-4" />
                El diferencial
              </span>
              <h2 className="mt-5 font-display text-3xl font-extrabold leading-tight tracking-tight text-slate-900 dark:text-white sm:text-4xl">
                Un resumen semanal,{' '}
                <span className="shimmer-text">generado con IA</span>
              </h2>
              <p className="mt-4 text-lg leading-relaxed text-slate-600 dark:text-slate-300">
                La IA lee lo publicado esa semana en Avisos, Calendario y Deberes, y te lo resume en tres líneas.
                Para el padre que no abre la app cada día, también llega como notificación.
              </p>
            </Reveal>

            <Reveal delay={120}>
              <ul className="mt-7 space-y-3">
                {[
                  'Solo lo importante: fechas límite, excursiones, exámenes.',
                  'Llega como notificación push, no tienes que abrir la app.',
                  'Si quieres detalle, un toque y vas al aviso original.',
                ].map((item) => (
                  <li key={item} className="flex items-start gap-3">
                    <CheckCircle2 className="mt-0.5 h-5 w-5 shrink-0 text-mint-500" />
                    <span className="text-slate-700 dark:text-slate-300">{item}</span>
                  </li>
                ))}
              </ul>
            </Reveal>
          </div>

          <Reveal delay={160}>
            <div className="relative mx-auto max-w-md">
              <div className="absolute -inset-4 -z-10 rounded-3xl bg-gradient-to-br from-brand-400/30 via-mint-400/30 to-rose-400/30 blur-2xl" />
              <div className="rounded-3xl border border-slate-200 bg-white p-5 shadow-2xl dark:border-slate-700 dark:bg-slate-900">
                <div className="flex items-center gap-2 border-b border-slate-100 pb-3 dark:border-slate-800">
                  <div className="grid h-9 w-9 place-items-center rounded-xl bg-gradient-to-br from-brand-400 via-mint-400 to-rose-400 text-white">
                    <Sparkles className="h-5 w-5" />
                  </div>
                  <div>
                    <p className="font-display text-sm font-bold text-slate-900 dark:text-white">Resumen de la semana</p>
                    <p className="text-xs text-slate-400">Generado automáticamente · 18 oct</p>
                  </div>
                </div>

                <p className="mt-4 text-sm leading-relaxed text-slate-700 dark:text-slate-200">
                  Esta semana hay <strong className="font-semibold text-slate-900 dark:text-white">3 cosas importantes</strong>:
                </p>

                <div className="mt-4 space-y-3">
                  <SummaryItem icon={<Calendar className="h-4 w-4" />} color="violet" text="Jueves: excursión al museo. Traer autorización firmada." />
                  <SummaryItem icon={<BookOpen className="h-4 w-4" />} color="mint" text="Viernes: examen de matemáticas (temas 3 y 4)." />
                  <SummaryItem icon={<Gift className="h-4 w-4" />} color="rose" text="Sábado: cumpleaños de Martina." />
                </div>

                <div className="mt-4 flex items-center gap-1.5 rounded-xl bg-mint-50 px-3 py-2 text-xs font-medium text-mint-700 dark:bg-mint-900/30 dark:text-mint-300">
                  <Bell className="h-3.5 w-3.5" />
                  Te avisaremos el miércoles por la mañana
                </div>
              </div>
            </div>
          </Reveal>
        </div>
      </div>
    </section>
  );
}

type ItemColor = 'brand' | 'mint' | 'rose' | 'violet';

const itemColors: Record<ItemColor, string> = {
  brand: 'bg-brand-100 text-brand-600 dark:bg-brand-900/40 dark:text-brand-300',
  mint: 'bg-mint-100 text-mint-600 dark:bg-mint-900/40 dark:text-mint-300',
  rose: 'bg-rose-100 text-rose-600 dark:bg-rose-900/40 dark:text-rose-300',
  violet: 'bg-violet-100 text-violet-600 dark:bg-violet-900/40 dark:text-violet-300',
};

function SummaryItem({ icon, color, text }: { icon: React.ReactNode; color: ItemColor; text: string }) {
  return (
    <div className="flex items-start gap-2.5">
      <div className={`grid h-8 w-8 shrink-0 place-items-center rounded-lg ${itemColors[color]}`}>{icon}</div>
      <p className="text-sm leading-snug text-slate-700 dark:text-slate-300">{text}</p>
    </div>
  );
}

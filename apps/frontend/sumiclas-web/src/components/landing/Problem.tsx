import { MessageCircle, AlertTriangle, Repeat2, EyeOff } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';

const problems = [
  {
    icon: Repeat2,
    title: 'Mensajes duplicados',
    text: 'La misma pregunta se contesta cinco veces y se vuelve imposible encontrar nada.',
  },
  {
    icon: EyeOff,
    title: 'Lo importante se pierde',
    text: 'La autorización de la excursión queda enterrada entre chistes y audios de 3 minutos.',
  },
  {
    icon: AlertTriangle,
    title: 'Avisos que no llegan',
    text: 'No todos los padres están en el grupo, y los que están no lo leen entero.',
  },
];

export function Problem() {
  return (
    <section className="relative py-20 sm:py-28">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="grid gap-12 lg:grid-cols-2 lg:gap-16 lg:items-center">
          <Reveal className="order-2 lg:order-1">
            <div className="relative mx-auto max-w-md">
              <div className="rounded-3xl border border-slate-200 bg-slate-50 p-4 shadow-xl dark:border-slate-700 dark:bg-slate-900">
                <div className="mb-3 flex items-center gap-2 border-b border-slate-200 pb-3 dark:border-slate-700">
                  <div className="grid h-10 w-10 place-items-center rounded-full bg-emerald-500/15 text-emerald-600 dark:text-emerald-400">
                    <MessageCircle className="h-5 w-5" />
                  </div>
                  <div>
                    <p className="text-sm font-bold text-slate-900 dark:text-white">Grupo de WhatsApp · 2ºA</p>
                    <p className="text-xs text-slate-400">24 participantes · 187 sin leer</p>
                  </div>
                </div>
                <div className="space-y-2.5">
                  <Bubble side="left" tone="muted">¿Alguien sabe qué páginas eran de mates?</Bubble>
                  <Bubble side="left" tone="muted">Creo que la 14 y 15</Bubble>
                  <Bubble side="left" tone="muted">No, era la 12 y 13</Bubble>
                  <Bubble side="right">Y la autorización del museo ¿para cuándo era?</Bubble>
                  <Bubble side="left" tone="lost">📅 Entregar antes del viernes</Bubble>
                  <Bubble side="left" tone="muted">Pero ¿alguien la tiene?</Bubble>
                  <Bubble side="left" tone="muted">Yo la subí ayer</Bubble>
                  <Bubble side="left" tone="muted">No la veo 😅</Bubble>
                  <div className="flex items-center justify-center gap-1.5 py-2 text-xs font-medium text-rose-500">
                    <AlertTriangle className="h-4 w-4" /> La autorización quedó enterrada
                  </div>
                </div>
              </div>
            </div>
          </Reveal>

          <div className="order-1 lg:order-2">
            <Reveal>
              <p className="font-display text-sm font-bold uppercase tracking-wider text-rose-500 dark:text-rose-400">
                El problema
              </p>
              <h2 className="mt-3 font-display text-3xl font-extrabold leading-tight tracking-tight text-slate-900 dark:text-white sm:text-4xl">
                El grupo de WhatsApp del cole se ha vuelto un caos
              </h2>
              <p className="mt-4 text-lg leading-relaxed text-slate-600 dark:text-slate-300">
                Empezó con buena intención. Ahora hay cientos de mensajes al día, la mitad duplicados,
                y lo importante se pierde entre el ruido.
              </p>
            </Reveal>

            <div className="mt-8 space-y-5">
              {problems.map((p, i) => (
                <Reveal key={p.title} delay={i * 100}>
                  <div className="flex items-start gap-4 rounded-2xl border border-slate-200 bg-white p-4 transition-colors hover:border-rose-200 hover:bg-rose-50/50 dark:border-slate-700 dark:bg-slate-900 dark:hover:border-rose-800/50 dark:hover:bg-rose-900/10">
                    <div className="grid h-11 w-11 shrink-0 place-items-center rounded-xl bg-rose-100 text-rose-600 dark:bg-rose-900/40 dark:text-rose-300">
                      <p.icon className="h-5 w-5" />
                    </div>
                    <div>
                      <h3 className="font-semibold text-slate-900 dark:text-white">{p.title}</h3>
                      <p className="mt-1 text-sm leading-relaxed text-slate-600 dark:text-slate-400">{p.text}</p>
                    </div>
                  </div>
                </Reveal>
              ))}
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

function Bubble({
  side,
  tone,
  children,
}: {
  side: 'left' | 'right';
  tone?: 'muted' | 'lost';
  children: React.ReactNode;
}) {
  const isRight = side === 'right';
  return (
    <div className={`flex ${isRight ? 'justify-end' : 'justify-start'}`}>
      <div
        className={`max-w-[80%] rounded-2xl px-3 py-1.5 text-xs ${
          isRight
            ? 'rounded-br-sm bg-brand-500 text-white'
            : tone === 'lost'
            ? 'rounded-bl-sm border border-rose-200 bg-rose-50 text-rose-700 dark:border-rose-800/50 dark:bg-rose-900/20 dark:text-rose-300'
            : 'rounded-bl-sm bg-white text-slate-600 dark:bg-slate-800 dark:text-slate-300'
        }`}
      >
        {children}
      </div>
    </div>
  );
}

import { Star, Quote } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';

const reviews = [
  {
    name: 'Laura M.',
    role: 'madre de 2º de Primaria',
    text: 'Antes se me pasaban las autorizaciones de las excursiones entre tanto mensaje. Ahora las veo directas en Avisos.',
    color: 'rose',
  },
  {
    name: 'Javier P.',
    role: 'padre de 4º de Primaria',
    text: 'El resumen del domingo me ha cambiado la vida. Sé qué hay que firmar sin leer 200 mensajes.',
    color: 'brand',
  },
  {
    name: 'Cristina S.',
    role: 'madre de 1º de ESO',
    text: 'Los gastos del regalo de la profe por fin están claros. Veo quién falta de un vistazo, sin ir preguntando.',
    color: 'mint',
  },
  {
    name: 'David R.',
    role: 'padre de 3º de Infantil',
    text: 'Mi mujer y yo tenemos la misma info. Antes uno se enteraba y el otro no. Ahora los dos vemos lo mismo.',
    color: 'sun',
  },
];

type Color = 'rose' | 'brand' | 'mint' | 'sun';

const avatarColors: Record<Color, string> = {
  rose: 'bg-rose-100 text-rose-700 dark:bg-rose-900/50 dark:text-rose-300',
  brand: 'bg-brand-100 text-brand-700 dark:bg-brand-900/50 dark:text-brand-300',
  mint: 'bg-mint-100 text-mint-700 dark:bg-mint-900/50 dark:text-mint-300',
  sun: 'bg-sun-100 text-sun-700 dark:bg-sun-900/50 dark:text-sun-300',
};

export function SocialProof() {
  return (
    <section id="opiniones" className="relative py-20 sm:py-28">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {/* Aggregate */}
        <Reveal>
          <div className="mx-auto max-w-2xl text-center">
            <div className="flex items-center justify-center gap-2">
              <span className="font-display text-5xl font-extrabold text-slate-900 dark:text-white">4.8</span>
              <span className="font-display text-2xl font-bold text-slate-400">/5</span>
            </div>
            <div className="mt-2 flex items-center justify-center gap-1">
              {[0, 1, 2, 3, 4].map((i) => (
                <Star key={i} className="h-5 w-5 fill-sun-400 text-sun-400" />
              ))}
            </div>
            <p className="mt-3 text-sm text-slate-500 dark:text-slate-400">
              basado en <strong className="font-semibold text-slate-700 dark:text-slate-200">312 familias</strong>
            </p>
          </div>
        </Reveal>

        {/* Review cards */}
        <div className="mt-14 grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {reviews.map((r, i) => (
            <Reveal key={r.name} delay={(i % 4) * 90}>
              <figure className="group flex h-full flex-col rounded-2xl border border-slate-200 bg-white p-6 shadow-sm transition-all duration-300 hover:-translate-y-1.5 hover:shadow-xl dark:border-slate-700 dark:bg-slate-900">
                <Quote className="h-8 w-8 text-slate-200 transition-colors group-hover:text-brand-300 dark:text-slate-700" />
                <blockquote className="mt-3 flex-1 text-sm leading-relaxed text-slate-700 dark:text-slate-300">
                  “{r.text}”
                </blockquote>
                <div className="mt-5 flex items-center gap-1">
                  {[0, 1, 2, 3, 4].map((s) => (
                    <Star key={s} className="h-4 w-4 fill-sun-400 text-sun-400" />
                  ))}
                </div>
                <figcaption className="mt-4 flex items-center gap-3 border-t border-slate-100 pt-4 dark:border-slate-800">
                  <div className={`grid h-10 w-10 place-items-center rounded-full font-display text-sm font-bold ${avatarColors[r.color as Color]}`}>
                    {r.name.charAt(0)}
                  </div>
                  <div>
                    <p className="text-sm font-bold text-slate-900 dark:text-white">{r.name}</p>
                    <p className="text-xs text-slate-500 dark:text-slate-400">{r.role}</p>
                  </div>
                </figcaption>
              </figure>
            </Reveal>
          ))}
        </div>
      </div>
    </section>
  );
}

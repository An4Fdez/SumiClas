import { Reveal } from './Reveal';
import { featureMeta, type FeatureKey } from './featureMeta';

type Color = 'rose' | 'brand' | 'mint' | 'sun' | 'violet' | 'orange';

const features: {
  key: FeatureKey;
  title: string;
  text: string;
  color: Color;
}[] = [
  {
    key: 'avisos',
    title: 'Avisos',
    text: 'Listado ordenado por fecha. Lo prioritario destaca arriba. Solo publica lo oficial el tutor, el delegado o el AMPA.',
    color: 'rose',
  },
  {
    key: 'calendario',
    title: 'Calendario',
    text: 'Excursiones, reuniones y actividades por clase o colegio. Cumpleaños como capa opcional, con icono propio.',
    color: 'violet',
  },
  {
    key: 'deberes',
    title: 'Deberes',
    text: 'Pizarra organizada por día y asignatura, con hilo corto de comentarios para dudas puntuales. No es un chat libre.',
    color: 'mint',
  },
  {
    key: 'gastos',
    title: 'Gastos de clase',
    text: 'Regalo colectivo, material… defines quién participa y cada uno marca su parte como pagada. Sin mover dinero.',
    color: 'sun',
  },
  {
    key: 'perdidos',
    title: 'Objetos perdidos',
    text: 'Tablón simple de buscado / encontrado, visible para toda la clase o todo el colegio según lo marques.',
    color: 'orange',
  },
  {
    key: 'resumen',
    title: 'Resumen semanal (IA)',
    text: 'Cada semana se genera un resumen a partir de lo publicado. No tienes que leerlo todo: la IA te lo resume.',
    color: 'brand',
  },
];

const colorClasses: Record<Color, { bg: string; text: string; ring: string; glow: string }> = {
  rose: {
    bg: 'bg-rose-100 dark:bg-rose-900/40',
    text: 'text-rose-600 dark:text-rose-300',
    ring: 'group-hover:border-rose-300 dark:group-hover:border-rose-700',
    glow: 'group-hover:shadow-rose-500/20',
  },
  brand: {
    bg: 'bg-brand-100 dark:bg-brand-900/40',
    text: 'text-brand-600 dark:text-brand-300',
    ring: 'group-hover:border-brand-300 dark:group-hover:border-brand-700',
    glow: 'group-hover:shadow-brand-500/20',
  },
  mint: {
    bg: 'bg-mint-100 dark:bg-mint-900/40',
    text: 'text-mint-600 dark:text-mint-300',
    ring: 'group-hover:border-mint-300 dark:group-hover:border-mint-700',
    glow: 'group-hover:shadow-mint-500/20',
  },
  sun: {
    bg: 'bg-sun-100 dark:bg-sun-900/40',
    text: 'text-sun-600 dark:text-sun-300',
    ring: 'group-hover:border-sun-300 dark:group-hover:border-sun-700',
    glow: 'group-hover:shadow-sun-500/20',
  },
  violet: {
    bg: 'bg-violet-100 dark:bg-violet-900/40',
    text: 'text-violet-600 dark:text-violet-300',
    ring: 'group-hover:border-violet-300 dark:group-hover:border-violet-700',
    glow: 'group-hover:shadow-violet-500/20',
  },
  orange: {
    bg: 'bg-orange-100 dark:bg-orange-900/40',
    text: 'text-orange-600 dark:text-orange-300',
    ring: 'group-hover:border-orange-300 dark:group-hover:border-orange-700',
    glow: 'group-hover:shadow-orange-500/20',
  },
};

export function Features() {
  return (
    <section id="funcionalidades" className="relative py-20 sm:py-28">
      {/* soft bg */}
      <div className="pointer-events-none absolute inset-0 -z-10 bg-gradient-to-b from-slate-50 to-white dark:from-slate-950 dark:to-slate-900" />

      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <Reveal>
            <p className="font-display text-sm font-bold uppercase tracking-wider text-brand-500 dark:text-brand-400">
              Cómo funciona
            </p>
            <h2 className="mt-3 font-display text-3xl font-extrabold leading-tight tracking-tight text-slate-900 dark:text-white sm:text-4xl">
              Todo lo de la clase, organizado en seis sitios
            </h2>
            <p className="mt-4 text-lg leading-relaxed text-slate-600 dark:text-slate-300">
              Cada cosa en su sitio. Nada se duplica, nada se pierde. Y al final de la semana, un resumen que te ahorra leerlo todo.
            </p>
          </Reveal>
        </div>

        <div className="mt-14 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {features.map((f, i) => {
            const meta = featureMeta[f.key];
            const Icon = meta.icon;
            const c = colorClasses[f.color];
            return (
              <Reveal key={f.key} delay={(i % 3) * 100}>
                <div
                  className={`group h-full rounded-2xl border border-slate-200 bg-white p-6 shadow-sm transition-all duration-300 hover:-translate-y-1.5 hover:shadow-xl dark:border-slate-700 dark:bg-slate-900 ${c.ring} ${c.glow}`}
                >
                  <div className={`grid h-12 w-12 place-items-center rounded-xl ${c.bg} ${c.text} transition-transform duration-300 group-hover:scale-110`}>
                    <Icon className="h-6 w-6" strokeWidth={2} />
                  </div>
                  <h3 className="mt-5 font-display text-lg font-bold text-slate-900 dark:text-white">{f.title}</h3>
                  <p className="mt-2 text-sm leading-relaxed text-slate-600 dark:text-slate-400">{f.text}</p>
                </div>
              </Reveal>
            );
          })}
        </div>
      </div>
    </section>
  );
}

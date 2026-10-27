import { useState } from 'react';
import { ChevronDown } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';

const faqs = [
  {
    q: '¿Esto sustituye al grupo de WhatsApp del cole?',
    a: 'Sí. SumiClas está pensado para sustituir al grupo de WhatsApp de la clase como canal principal de coordinación. Lo importante no se pierde entre el ruido, y el resumen semanal te ahorra leerlo todo. Si queréis, podéis seguir con un grupo para lo social, pero lo organizativo queda aquí.',
  },
  {
    q: '¿Es gratis?',
    a: 'SumiClas es completamente gratis para las familias. Si en el futuro se introduce un plan de pago, siempre habrá una versión gratuita con las funciones esenciales.',
  },
  {
    q: '¿Qué pasa con los datos de mi hijo?',
    a: 'Los datos de los alumnos se muestran de forma mínima: nombre e inicial del primer apellido, sin fecha de nacimiento ni fotos por defecto. Los cumpleaños son opcionales y solo guardan día y mes, nunca el año. Tú decides qué compartes.',
  },
  {
    q: '¿Puede unirse el profesor o el AMPA?',
    a: 'Por supuesto. Los roles dentro de una clase son profesor/tutor, delegado y participante (el resto de padres). A nivel de colegio existe además un rol de administrador para el AMPA o dirección, que puede publicar avisos visibles para todas las clases.',
  },
];

export function Faq() {
  return (
    <section id="faq" className="relative py-20 sm:py-24">
      <div className="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8">
        <Reveal>
          <div className="text-center">
            <p className="font-display text-sm font-bold uppercase tracking-wider text-mint-600 dark:text-mint-400">
              Preguntas frecuentes
            </p>
            <h2 className="mt-3 font-display text-3xl font-extrabold leading-tight tracking-tight text-slate-900 dark:text-white sm:text-4xl">
              Lo que sueña preguntar antes de empezar
            </h2>
          </div>
        </Reveal>

        <div className="mt-10 space-y-3">
          {faqs.map((f, i) => (
            <Reveal key={f.q} delay={i * 80}>
              <FaqItem q={f.q} a={f.a} />
            </Reveal>
          ))}
        </div>
      </div>
    </section>
  );
}

function FaqItem({ q, a }: { q: string; a: string }) {
  const [open, setOpen] = useState(false);
  return (
    <div
      className={`overflow-hidden rounded-2xl border transition-colors ${
        open
          ? 'border-brand-200 bg-brand-50/40 dark:border-brand-700/50 dark:bg-brand-900/15'
          : 'border-slate-200 bg-white dark:border-slate-700 dark:bg-slate-900'
      }`}
    >
      <button
        onClick={() => setOpen((o) => !o)}
        className="flex w-full items-center justify-between gap-4 px-5 py-4 text-left"
        aria-expanded={open}
      >
        <span className="font-display text-base font-bold text-slate-900 dark:text-white">{q}</span>
        <ChevronDown
          className={`h-5 w-5 shrink-0 text-slate-400 transition-transform duration-300 ${open ? 'rotate-180' : ''}`}
        />
      </button>
      <div
        className={`grid transition-all duration-300 ease-out ${
          open ? 'grid-rows-[1fr] opacity-100' : 'grid-rows-[0fr] opacity-0'
        }`}
      >
        <div className="overflow-hidden">
          <p className="px-5 pb-5 text-sm leading-relaxed text-slate-600 dark:text-slate-400">{a}</p>
        </div>
      </div>
    </div>
  );
}

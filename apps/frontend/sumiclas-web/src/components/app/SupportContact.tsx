import { Mail } from 'lucide-react';
import { CONTACT_EMAIL } from '@/lib/legal';

export function SupportContact() {
  return (
    <div>
      <h1 className="font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
        Ayuda y soporte
      </h1>

      <div className="mt-6 rounded-2xl border border-slate-200 bg-white p-5 dark:border-slate-700 dark:bg-slate-900">
        <p className="text-sm leading-relaxed text-slate-600 dark:text-slate-300">
          Estamos encantados de ayudarte con cualquier duda, problema o sugerencia sobre SumiClas. Cuéntanos qué te
          pasa o qué echas en falta, sin prisa y con toda confianza. Solemos responder por correo electrónico, así que
          escríbenos cuando quieras. 😊
        </p>

        <div className="mt-5 flex items-center gap-3 rounded-xl border border-slate-100 bg-slate-50 px-4 py-3 dark:border-slate-800 dark:bg-slate-800/60">
          <Mail className="h-4 w-4 shrink-0 text-slate-400" aria-hidden="true" />
          <a
            href={`mailto:${CONTACT_EMAIL}`}
            className="text-sm font-semibold text-slate-900 underline-offset-2 hover:underline dark:text-white"
          >
            {CONTACT_EMAIL}
          </a>
        </div>
      </div>
    </div>
  );
}

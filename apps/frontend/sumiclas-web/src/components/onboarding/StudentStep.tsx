import { useState, type FormEvent } from 'react';
import { UserRound, ArrowRight } from 'lucide-react';
import { getApiErrorMessage } from '@/lib/apiError';

export type StudentData = {
  firstName: string;
  firstSurname: string;
  secondSurname?: string;
};

const fieldWrapperClasses =
  'flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40';
const labelClasses = 'mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300';
const inputClasses =
  'w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200';

export function StudentStep({
  registeredStudent,
  onConfirm,
}: {
  registeredStudent: string | null;
  onConfirm: (data: StudentData) => Promise<void>;
}) {
  const [firstName, setFirstName] = useState('');
  const [firstSurname, setFirstSurname] = useState('');
  const [secondSurname, setSecondSurname] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!registeredStudent && (!firstName.trim() || !firstSurname.trim())) return;
    setSubmitting(true);
    setError(null);
    try {
      await onConfirm({
        firstName: firstName.trim(),
        firstSurname: firstSurname.trim(),
        secondSurname: secondSurname.trim() || undefined,
      });
    } catch (err) {
      setError(getApiErrorMessage(err));
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {registeredStudent ? (
        <p className="rounded-xl border border-mint-200 bg-mint-50/60 p-4 text-sm text-slate-700 dark:border-mint-700/40 dark:bg-mint-900/15 dark:text-slate-300">
          <strong className="font-semibold text-slate-900 dark:text-white">{registeredStudent}</strong> ya está dado de
          alta. Falta matricularlo/a en la clase elegida.
        </p>
      ) : (
        <>
          <div>
            <label htmlFor="student-name" className={labelClasses}>
              Nombre del alumno/a
            </label>
            <div className={fieldWrapperClasses}>
              <UserRound className="h-4 w-4 shrink-0 text-slate-400" />
              <input
                id="student-name"
                type="text"
                required
                maxLength={100}
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                placeholder="Martina"
                className={inputClasses}
              />
            </div>
          </div>

          <div>
            <label htmlFor="student-last-name" className={labelClasses}>
              Primer apellido
            </label>
            <div className={fieldWrapperClasses}>
              <UserRound className="h-4 w-4 shrink-0 text-slate-400" />
              <input
                id="student-last-name"
                type="text"
                required
                maxLength={100}
                value={firstSurname}
                onChange={(e) => setFirstSurname(e.target.value)}
                placeholder="Ruiz"
                className={inputClasses}
              />
            </div>
            <p className="mt-1.5 text-xs text-slate-500 dark:text-slate-400">
              Solo se mostrará a otras familias como "{firstName || 'Nombre'}{' '}
              {firstSurname ? `${firstSurname.charAt(0)}.` : 'A.'}"
            </p>
          </div>

          <div>
            <label htmlFor="student-second-last-name" className={labelClasses}>
              Segundo apellido <span className="font-normal text-slate-400">(opcional)</span>
            </label>
            <div className={fieldWrapperClasses}>
              <UserRound className="h-4 w-4 shrink-0 text-slate-400" />
              <input
                id="student-second-last-name"
                type="text"
                maxLength={100}
                value={secondSurname}
                onChange={(e) => setSecondSurname(e.target.value)}
                placeholder="Gil"
                className={inputClasses}
              />
            </div>
          </div>
        </>
      )}

      {error && (
        <p role="alert" className="text-sm font-semibold text-rose-600 dark:text-rose-400">
          {error}
        </p>
      )}

      <button
        type="submit"
        disabled={submitting}
        className="mt-2 flex w-full items-center justify-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-brand-500/25 transition-transform hover:scale-[1.02] active:scale-95 disabled:opacity-60"
      >
        {submitting ? 'Guardando…' : registeredStudent ? 'Matricular y entrar a Avisos' : 'Añadir alumno y entrar a Avisos'}
        <ArrowRight className="h-4 w-4" />
      </button>
    </form>
  );
}

import { Check } from 'lucide-react';

const steps = ['Colegio', 'Clase', 'Alumno'];

export function StepIndicator({ current }: { current: number }) {
  return (
    <ol className="flex items-center justify-center gap-2 sm:gap-4">
      {steps.map((label, i) => {
        const step = i + 1;
        const state = step < current ? 'done' : step === current ? 'active' : 'upcoming';
        return (
          <li key={label} className="flex items-center gap-2 sm:gap-4">
            <div className="flex items-center gap-2">
              <span
                className={`grid h-8 w-8 shrink-0 place-items-center rounded-full text-sm font-bold transition-colors ${
                  state === 'done'
                    ? 'bg-brand-500 text-white'
                    : state === 'active'
                    ? 'bg-gradient-to-br from-brand-500 via-mint-500 to-rose-500 text-white shadow-md shadow-brand-500/30'
                    : 'bg-slate-100 text-slate-400 dark:bg-slate-800 dark:text-slate-500'
                }`}
              >
                {state === 'done' ? <Check className="h-4 w-4" /> : step}
              </span>
              <span
                className={`hidden text-sm font-semibold sm:inline ${
                  state === 'upcoming' ? 'text-slate-400 dark:text-slate-600' : 'text-slate-900 dark:text-white'
                }`}
              >
                {label}
              </span>
            </div>
            {step < steps.length && <span className="h-px w-6 bg-slate-200 dark:bg-slate-700 sm:w-10" />}
          </li>
        );
      })}
    </ol>
  );
}

import { Sparkles, CheckCircle2 } from 'lucide-react';
import type { WeeklySummaryState } from '@/hooks/useClassWeeklySummary';
import { formatRelativeDate } from '@/lib/date';

const SUBTITLE = 'Generado con IA';
const LOADING_MESSAGE = 'Preparando el resumen de la semana…';
const EMPTY_MESSAGE = 'Sin novedades esta semana.';

export function WeeklySummaryCard({ state, onGenerate }: { state: WeeklySummaryState; onGenerate: () => void }) {
  if (state.status === 'idle') return null;

  const generatedAt = state.status === 'ready' ? state.summary.generatedAt : undefined;

  return (
    <div className="relative overflow-hidden rounded-3xl border border-slate-200 bg-white p-5 shadow-sm dark:border-slate-700 dark:bg-slate-900 sm:p-6">
      <div className="flex items-center gap-2.5">
        <div className="grid h-10 w-10 shrink-0 place-items-center rounded-xl bg-gradient-to-br from-brand-400 via-mint-400 to-rose-400 text-white">
          <Sparkles className="h-5 w-5" />
        </div>
        <div>
          <p className="font-display text-base font-bold shimmer-text">Resumen de la semana</p>
          <p className="text-xs text-slate-400 dark:text-slate-500">
            {generatedAt ? `${SUBTITLE} · ${formatRelativeDate(generatedAt)}` : SUBTITLE}
          </p>
        </div>
      </div>

      {state.status === 'loading' && (
        <p role="status" className="mt-4 text-sm text-slate-500 dark:text-slate-400">
          {LOADING_MESSAGE}
        </p>
      )}

      {state.status === 'error' && (
        <div role="alert" className="mt-4">
          <p className="text-sm text-slate-700 dark:text-slate-300">{state.message}</p>
          <button
            type="button"
            onClick={onGenerate}
            className="mt-3 text-sm font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
          >
            Generar resumen
          </button>
        </div>
      )}

      {state.status === 'ready' && state.summary.lines.length === 0 && (
        <p className="mt-4 text-sm text-slate-500 dark:text-slate-400">{EMPTY_MESSAGE}</p>
      )}

      {state.status === 'ready' && state.summary.lines.length > 0 && (
        <ul className="mt-4 space-y-2.5">
          {state.summary.lines.map((line, index) => (
            <li key={`${index}-${line}`} className="flex items-start gap-2.5">
              <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-mint-500" />
              <span className="text-sm leading-snug text-slate-700 dark:text-slate-300">{line}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

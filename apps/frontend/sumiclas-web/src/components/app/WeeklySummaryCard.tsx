import { Sparkles, CheckCircle2 } from 'lucide-react';

export function WeeklySummaryCard({ fecha, puntos }: { fecha: string; puntos: string[] }) {
  return (
    <div className="relative overflow-hidden rounded-3xl border border-slate-200 bg-white p-5 shadow-sm dark:border-slate-700 dark:bg-slate-900 sm:p-6">
      <div className="flex items-center gap-2.5">
        <div className="grid h-10 w-10 shrink-0 place-items-center rounded-xl bg-gradient-to-br from-brand-400 via-mint-400 to-rose-400 text-white">
          <Sparkles className="h-5 w-5" />
        </div>
        <div>
          <p className="font-display text-base font-bold shimmer-text">Resumen de la semana</p>
          <p className="text-xs text-slate-400 dark:text-slate-500">Generado con IA · {fecha}</p>
        </div>
      </div>

      <ul className="mt-4 space-y-2.5">
        {puntos.map((p) => (
          <li key={p} className="flex items-start gap-2.5">
            <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-mint-500" />
            <span className="text-sm leading-snug text-slate-700 dark:text-slate-300">{p}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}

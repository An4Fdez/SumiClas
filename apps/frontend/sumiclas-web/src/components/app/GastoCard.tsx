import { ArrowUpRight, Check } from 'lucide-react';
import { formatFechaRelativa } from '@/lib/date';
import type { GastoClase } from '@/data/gastosMock';

export function GastoCard({
  gasto,
  yoParticipo,
  yoHePagado,
  numPagados,
  importePorFamilia,
  onAbrir,
  onTogglePago,
}: {
  gasto: GastoClase;
  yoParticipo: boolean;
  yoHePagado: boolean;
  numPagados: number;
  importePorFamilia: number;
  onAbrir: () => void;
  onTogglePago: () => void;
}) {
  return (
    <div className="relative w-full rounded-2xl border border-slate-200 bg-white shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md dark:border-slate-700 dark:bg-slate-900">
      {yoParticipo && (
        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            onTogglePago();
          }}
          aria-label={yoHePagado ? 'Marcar como pendiente' : 'Marcar como pagado'}
          title={yoHePagado ? 'Marcar como pendiente' : 'Marcar como pagado'}
          className={`absolute right-3 top-3 z-10 grid h-6 w-6 place-items-center rounded-full border transition-colors ${
            yoHePagado
              ? 'border-mint-500 bg-mint-500 text-white'
              : 'border-slate-300 hover:bg-slate-100 dark:border-slate-600 dark:hover:bg-slate-800'
          }`}
        >
          {yoHePagado && <Check className="h-3.5 w-3.5" />}
        </button>
      )}

      <button type="button" onClick={onAbrir} className="w-full p-4 pr-11 text-left">
        <h3 className="font-display text-base font-bold text-slate-900 dark:text-white">{gasto.concepto}</h3>
        <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">
          {numPagados}/{gasto.participantesIds.length} han pagado · {importePorFamilia.toFixed(2)} € por familia
        </p>

        <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-xs text-slate-500 dark:text-slate-400">
          <span>
            {gasto.autorNombre} · {gasto.autorRol}
          </span>
          <span>{formatFechaRelativa(gasto.fecha)}</span>
        </div>

        {gasto.eventoRelacionado && (
          <span className="mt-3 inline-flex items-center gap-1 text-xs font-semibold text-violet-600 dark:text-violet-300">
            <ArrowUpRight className="h-3.5 w-3.5" />
            Ver evento: {gasto.eventoRelacionado.titulo}
          </span>
        )}
      </button>
    </div>
  );
}

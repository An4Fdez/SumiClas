import { useRef, type MouseEvent } from 'react';
import { ArrowUpRight, Check, X } from 'lucide-react';
import { Link } from 'react-router-dom';
import { formatFechaRelativa } from '@/lib/date';
import type { GastoClase } from '@/data/gastosMock';

export type ParticipanteEstado = {
  id: string;
  nombre: string;
  pagado: boolean;
  esYo: boolean;
};

export function GastoDetalleModal({
  gasto,
  importePorFamilia,
  participantes,
  onTogglePago,
  onClose,
}: {
  gasto: GastoClase;
  importePorFamilia: number;
  participantes: ParticipanteEstado[];
  onTogglePago: () => void;
  onClose: () => void;
}) {
  const mouseDownEnFondo = useRef(false);
  const yo = participantes.find((p) => p.esYo);

  return (
    <div
      className="fixed inset-0 z-50 flex items-end justify-center bg-slate-900/40 p-0 backdrop-blur-sm sm:items-center sm:p-4"
      onMouseDown={(e: MouseEvent) => {
        mouseDownEnFondo.current = e.target === e.currentTarget;
      }}
      onClick={(e: MouseEvent) => {
        if (mouseDownEnFondo.current && e.target === e.currentTarget) onClose();
      }}
    >
      <div
        className="max-h-[85vh] w-full max-w-lg overflow-y-auto rounded-t-3xl border border-slate-200 bg-white p-6 shadow-xl dark:border-slate-700 dark:bg-slate-900 sm:rounded-3xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-start justify-between gap-3">
          <h2 className="font-display text-xl font-extrabold text-slate-900 dark:text-white">{gasto.concepto}</h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>
        <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
          {gasto.autorNombre} · {gasto.autorRol} · {formatFechaRelativa(gasto.fecha)}
        </p>

        <div className="mt-4 space-y-1 text-sm text-slate-700 dark:text-slate-300">
          <p>Importe total: {gasto.importeTotal.toFixed(2)} €</p>
          <p>Por familia: {importePorFamilia.toFixed(2)} €</p>
        </div>

        {gasto.eventoRelacionado && (
          <Link
            to={`/calendario?evento=${gasto.eventoRelacionado.id}`}
            className="mt-4 inline-flex items-center gap-1.5 rounded-xl border border-violet-200 px-3 py-2 text-sm font-semibold text-violet-700 transition-colors hover:bg-violet-50 dark:border-violet-800 dark:text-violet-300 dark:hover:bg-violet-900/20"
          >
            <ArrowUpRight className="h-4 w-4" />
            Ver evento: {gasto.eventoRelacionado.titulo}
          </Link>
        )}

        {yo && (
          <button
            type="button"
            onClick={onTogglePago}
            className={`mt-4 inline-flex items-center gap-1.5 rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
              yo.pagado
                ? 'border-mint-500 bg-mint-50 text-mint-700 dark:border-mint-500 dark:bg-mint-900/20 dark:text-mint-300'
                : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
            }`}
          >
            <Check className="h-4 w-4" />
            {yo.pagado ? 'Has pagado' : 'Marcar como pagado'}
          </button>
        )}

        <div className="mt-6">
          <h3 className="text-sm font-bold text-slate-900 dark:text-white">Participantes</h3>
          <div className="mt-3 space-y-2">
            {participantes.map((p) => (
              <div
                key={p.id}
                className="flex items-center justify-between rounded-xl bg-slate-50 px-3 py-2 dark:bg-slate-800"
              >
                <span className="text-sm font-semibold text-slate-700 dark:text-slate-300">
                  {p.esYo ? 'Tú' : p.nombre}
                </span>
                <span
                  className={`text-xs font-bold ${
                    p.pagado ? 'text-mint-600 dark:text-mint-400' : 'text-slate-400 dark:text-slate-500'
                  }`}
                >
                  {p.pagado ? 'Pagado' : 'Pendiente'}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

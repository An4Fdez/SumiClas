import { useRef, type MouseEvent } from 'react';
import { AlertCircle, ArrowUpRight, X } from 'lucide-react';
import { Link } from 'react-router-dom';
import { badgeColorClasses } from '@/lib/colorClasses';
import { formatFechaRelativa } from '@/lib/date';
import type { Aviso } from '@/data/avisosMock';

export function AvisoDetalleModal({
  aviso,
  claseNombre,
  onClose,
}: {
  aviso: Aviso;
  claseNombre?: string;
  onClose: () => void;
}) {
  const alcanceColor = aviso.alcance === 'colegio' ? badgeColorClasses.brand : badgeColorClasses.violet;
  const alcanceLabel = aviso.alcance === 'colegio' ? 'Colegio' : claseNombre ?? 'Clase';
  const mouseDownEnFondo = useRef(false);

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
          <div className="flex flex-wrap items-center gap-2">
            <span className={`rounded-full px-2.5 py-0.5 text-[11px] font-bold ${alcanceColor.bg} ${alcanceColor.text}`}>
              {alcanceLabel}
            </span>
            {aviso.prioridad === 'alta' && (
              <span className="inline-flex items-center gap-1 rounded-full bg-rose-100 px-2 py-0.5 text-[11px] font-bold text-rose-700 dark:bg-rose-900/40 dark:text-rose-300">
                <AlertCircle className="h-3 w-3" />
                Urgente
              </span>
            )}
          </div>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <h2 className="mt-3 font-display text-xl font-extrabold text-slate-900 dark:text-white">{aviso.titulo}</h2>
        <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
          {aviso.autorNombre} · {aviso.autorRol} · {formatFechaRelativa(aviso.fecha)}
        </p>

        <p className="mt-4 whitespace-pre-line text-sm leading-relaxed text-slate-700 dark:text-slate-300">
          {aviso.cuerpo}
        </p>

        {aviso.eventoRelacionado && (
          <Link
            to={`/calendario?evento=${aviso.eventoRelacionado.id}`}
            className="mt-5 inline-flex items-center gap-1.5 rounded-xl border border-violet-200 px-3 py-2 text-sm font-semibold text-violet-700 transition-colors hover:bg-violet-50 dark:border-violet-800 dark:text-violet-300 dark:hover:bg-violet-900/20"
          >
            <ArrowUpRight className="h-4 w-4" />
            Ver evento: {aviso.eventoRelacionado.titulo}
          </Link>
        )}
      </div>
    </div>
  );
}

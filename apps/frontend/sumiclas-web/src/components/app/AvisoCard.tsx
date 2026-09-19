import { AlertCircle, ArrowUpRight, Circle } from 'lucide-react';
import { badgeColorClasses } from '@/lib/colorClasses';
import { formatFechaRelativa } from '@/lib/date';
import type { Aviso } from '@/data/avisosMock';

const prioridadEstilo: Record<Aviso['prioridad'], string> = {
  alta: 'ring-1 ring-rose-200 dark:ring-rose-900/50',
  normal: '',
  baja: 'opacity-75',
};

export function AvisoCard({
  aviso,
  claseNombre,
  onOpen,
  onToggleLeido,
}: {
  aviso: Aviso;
  claseNombre?: string;
  onOpen: () => void;
  onToggleLeido: () => void;
}) {
  const alcanceColor = aviso.alcance === 'colegio' ? badgeColorClasses.brand : badgeColorClasses.violet;
  const alcanceLabel = aviso.alcance === 'colegio' ? 'Colegio' : claseNombre ?? 'Clase';

  return (
    <div
      className={`relative w-full rounded-2xl border border-slate-200 bg-white shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md dark:border-slate-700 dark:bg-slate-900 ${prioridadEstilo[aviso.prioridad]}`}
    >
      <button
        type="button"
        onClick={(e) => {
          e.stopPropagation();
          onToggleLeido();
        }}
        aria-label={aviso.leido ? 'Marcar como no leído' : 'Marcar como leído'}
        title={aviso.leido ? 'Marcar como no leído' : 'Marcar como leído'}
        className="absolute right-3 top-3 z-10 rounded-full p-1.5 transition-colors hover:bg-slate-100 dark:hover:bg-slate-800"
      >
        {aviso.leido ? (
          <Circle className="h-4 w-4 text-slate-300 dark:text-slate-600" />
        ) : (
          <Circle className="h-4 w-4 fill-brand-500 text-brand-500" />
        )}
      </button>

      <button type="button" onClick={onOpen} className="w-full p-4 pr-11 text-left">
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

        <h3 className="mt-2.5 font-display text-base font-bold text-slate-900 dark:text-white">{aviso.titulo}</h3>
        <p className="mt-1 line-clamp-2 text-sm leading-snug text-slate-600 dark:text-slate-400">{aviso.cuerpo}</p>

        <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-xs text-slate-500 dark:text-slate-400">
          <span>
            {aviso.autorNombre} · {aviso.autorRol}
          </span>
          <span>{formatFechaRelativa(aviso.fecha)}</span>
        </div>

        {aviso.eventoRelacionado && (
          <span className="mt-3 inline-flex items-center gap-1 text-xs font-semibold text-violet-600 dark:text-violet-300">
            <ArrowUpRight className="h-3.5 w-3.5" />
            Ver evento: {aviso.eventoRelacionado.titulo}
          </span>
        )}
      </button>
    </div>
  );
}

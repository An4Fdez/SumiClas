import { useRef, type MouseEvent } from 'react';
import { Coins, MapPin, X } from 'lucide-react';
import { badgeColorClasses } from '@/lib/colorClasses';
import { formatDiaCompleto } from '@/lib/date';
import { tipoEventoMeta, type EventoCalendario } from '@/data/calendarioMock';

export function EventoDetalleModal({
  evento,
  claseNombre,
  onClose,
}: {
  evento: EventoCalendario;
  claseNombre?: string;
  onClose: () => void;
}) {
  const meta = tipoEventoMeta[evento.tipo];
  const Icon = meta.icon;
  const c = badgeColorClasses[meta.color];
  const alcanceLabel = evento.alcance === 'colegio' ? 'Colegio' : (claseNombre ?? 'Clase');
  const mouseDownEnFondo = useRef(false);

  const esCumpleanos = evento.tipo === 'cumpleanos';
  const fechaEvento = new Date(evento.fecha);
  const fechaTexto = esCumpleanos
    ? `${fechaEvento.toLocaleDateString('es-ES', { day: 'numeric', month: 'long' })} (cada año)`
    : formatDiaCompleto(fechaEvento);

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
          <span
            className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-[11px] font-bold ${c.bg} ${c.text}`}
          >
            <Icon className="h-3 w-3" />
            {meta.label}
          </span>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <h2 className="mt-3 font-display text-xl font-extrabold text-slate-900 dark:text-white">{evento.titulo}</h2>
        <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
          {evento.autorNombre} · {evento.autorRol} · {alcanceLabel}
        </p>

        <div className="mt-4 space-y-2 text-sm text-slate-700 dark:text-slate-300">
          <p>
            {fechaTexto}
            {evento.hora ? ` · ${evento.hora}` : ''}
          </p>
          {evento.ubicacion && (
            <p className="flex items-center gap-1.5">
              <MapPin className="h-4 w-4 shrink-0 text-slate-400" />
              {evento.ubicacion}
            </p>
          )}
          {evento.coste && (
            <p className="flex items-center gap-1.5">
              <Coins className="h-4 w-4 shrink-0 text-slate-400" />
              {evento.coste}
            </p>
          )}
        </div>
      </div>
    </div>
  );
}

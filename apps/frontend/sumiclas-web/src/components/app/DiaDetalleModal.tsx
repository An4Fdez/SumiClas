import { useRef, type MouseEvent } from 'react';
import { X } from 'lucide-react';
import { CalendarioAgendaDia } from '@/components/app/CalendarioAgendaDia';
import type { EventoCalendario } from '@/data/calendarioMock';

export function DiaDetalleModal({
  dia,
  eventos,
  claseNombreDeEvento,
  onAbrirEvento,
  onClose,
}: {
  dia: Date;
  eventos: EventoCalendario[];
  claseNombreDeEvento: (evento: EventoCalendario) => string | undefined;
  onAbrirEvento: (evento: EventoCalendario) => void;
  onClose: () => void;
}) {
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
        <div className="flex items-start justify-end">
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <CalendarioAgendaDia
          dia={dia}
          eventos={eventos}
          claseNombreDeEvento={claseNombreDeEvento}
          onAbrirEvento={onAbrirEvento}
          mostrarFecha
        />
      </div>
    </div>
  );
}

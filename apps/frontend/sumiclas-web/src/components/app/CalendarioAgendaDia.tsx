import { formatDiaCompleto } from '@/lib/date';
import { EventoAgendaRow } from '@/components/app/EventoAgendaRow';
import type { EventoCalendario } from '@/data/calendarioMock';

export function CalendarioAgendaDia({
  dia,
  eventos,
  claseNombreDeEvento,
  onAbrirEvento,
  mostrarFecha = true,
}: {
  dia: Date | null;
  eventos: EventoCalendario[];
  claseNombreDeEvento: (evento: EventoCalendario) => string | undefined;
  onAbrirEvento: (evento: EventoCalendario) => void;
  mostrarFecha?: boolean;
}) {
  if (!dia) return null;

  return (
    <div className={mostrarFecha ? 'mt-4' : ''}>
      {mostrarFecha && (
        <h2 className="font-display text-base font-bold text-slate-900 dark:text-white">{formatDiaCompleto(dia)}</h2>
      )}

      {eventos.length === 0 ? (
        <p className={`text-sm text-slate-500 dark:text-slate-400 ${mostrarFecha ? 'mt-2' : ''}`}>
          No hay eventos este día.
        </p>
      ) : (
        <div className={`space-y-2 ${mostrarFecha ? 'mt-3' : ''}`}>
          {eventos.map((evento) => (
            <EventoAgendaRow
              key={evento.id}
              evento={evento}
              alcanceLabel={evento.alcance === 'colegio' ? 'Colegio' : (claseNombreDeEvento(evento) ?? 'Clase')}
              onAbrir={() => onAbrirEvento(evento)}
            />
          ))}
        </div>
      )}
    </div>
  );
}

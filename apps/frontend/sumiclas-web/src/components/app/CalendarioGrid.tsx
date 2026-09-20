import { formatMesAnio, generarMatrizMes } from '@/lib/date';
import { CalendarioDiaCelda } from '@/components/app/CalendarioDiaCelda';
import { CalendarioNavHeader } from '@/components/app/CalendarioNavHeader';
import type { EventoCalendario } from '@/data/calendarioMock';

const diasSemana = ['L', 'M', 'X', 'J', 'V', 'S', 'D'];

export function CalendarioGrid({
  anio,
  mes,
  eventosDelDia,
  onCambiarMes,
  onHoy,
  onAbrirDia,
}: {
  anio: number;
  mes: number;
  eventosDelDia: (dia: Date) => EventoCalendario[];
  onCambiarMes: (delta: number) => void;
  onHoy: () => void;
  onAbrirDia: (dia: Date) => void;
}) {
  const semanas = generarMatrizMes(anio, mes);

  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-3 dark:border-slate-700 dark:bg-slate-900">
      <CalendarioNavHeader
        titulo={formatMesAnio(anio, mes)}
        onPrev={() => onCambiarMes(-1)}
        onNext={() => onCambiarMes(1)}
        onHoy={onHoy}
      />

      <div className="mt-3 grid grid-cols-7 gap-1 text-center text-[11px] font-semibold text-slate-400 dark:text-slate-500">
        {diasSemana.map((d) => (
          <div key={d}>{d}</div>
        ))}
      </div>

      <div className="mt-1 grid grid-cols-7 gap-1">
        {semanas.flatMap((semana, si) =>
          semana.map((dia, di) => (
            <CalendarioDiaCelda key={`${si}-${di}`} dia={dia} eventos={eventosDelDia(dia.fecha)} onAbrir={onAbrirDia} />
          ))
        )}
      </div>
    </div>
  );
}

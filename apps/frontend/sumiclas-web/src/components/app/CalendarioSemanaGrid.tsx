import { formatRangoSemana, generarSemana } from '@/lib/date';
import { CalendarioDiaCelda } from '@/components/app/CalendarioDiaCelda';
import { CalendarioNavHeader } from '@/components/app/CalendarioNavHeader';
import type { EventoCalendario } from '@/data/calendarioMock';

const diasSemana = ['L', 'M', 'X', 'J', 'V', 'S', 'D'];

export function CalendarioSemanaGrid({
  fechaReferencia,
  eventosDelDia,
  onCambiarSemana,
  onHoy,
  onAbrirDia,
}: {
  fechaReferencia: Date;
  eventosDelDia: (dia: Date) => EventoCalendario[];
  onCambiarSemana: (delta: number) => void;
  onHoy: () => void;
  onAbrirDia: (dia: Date) => void;
}) {
  const dias = generarSemana(fechaReferencia);

  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-3 dark:border-slate-700 dark:bg-slate-900">
      <CalendarioNavHeader
        titulo={formatRangoSemana(dias)}
        onPrev={() => onCambiarSemana(-1)}
        onNext={() => onCambiarSemana(1)}
        onHoy={onHoy}
      />

      <div className="mt-3 grid grid-cols-7 gap-1 text-center text-[11px] font-semibold text-slate-400 dark:text-slate-500">
        {diasSemana.map((d) => (
          <div key={d}>{d}</div>
        ))}
      </div>

      <div className="mt-1 grid grid-cols-7 gap-1">
        {dias.map((dia) => (
          <CalendarioDiaCelda key={dia.fecha.toISOString()} dia={dia} eventos={eventosDelDia(dia.fecha)} onAbrir={onAbrirDia} />
        ))}
      </div>
    </div>
  );
}

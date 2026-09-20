import { badgeColorClasses } from '@/lib/colorClasses';
import type { DiaCalendario } from '@/lib/date';
import { tipoEventoMeta, type EventoCalendario } from '@/data/calendarioMock';

export function CalendarioDiaCelda({
  dia,
  eventos,
  onAbrir,
}: {
  dia: DiaCalendario;
  eventos: EventoCalendario[];
  onAbrir: (dia: Date) => void;
}) {
  const eventosOrdenados = [...eventos].sort((a, b) => (a.hora ?? '99:99').localeCompare(b.hora ?? '99:99'));
  const primero = eventosOrdenados[0];
  const restantes = eventosOrdenados.length - 1;
  const colorPrimero = primero ? badgeColorClasses[tipoEventoMeta[primero.tipo].color] : null;

  return (
    <button
      type="button"
      onClick={() => onAbrir(dia.fecha)}
      className={`flex min-h-[60px] w-full flex-col items-start gap-1 rounded-lg p-1.5 text-left transition-colors ${
        !dia.enMesActual ? 'opacity-40' : ''
      } ${dia.esHoy ? 'bg-brand-50 dark:bg-brand-900/20' : 'hover:bg-slate-100 dark:hover:bg-slate-800'}`}
    >
      <span
        className={`text-xs font-semibold ${
          dia.esHoy ? 'text-brand-700 dark:text-brand-300' : 'text-slate-700 dark:text-slate-300'
        }`}
      >
        {dia.fecha.getDate()}
      </span>

      {primero && colorPrimero && (
        <span
          className={`w-full truncate rounded px-1 py-0.5 text-[10px] font-semibold leading-tight ${colorPrimero.bg} ${colorPrimero.text}`}
        >
          {primero.titulo}
        </span>
      )}
      {restantes > 0 && <span className="text-[10px] font-semibold text-slate-400 dark:text-slate-500">+{restantes}</span>}
    </button>
  );
}

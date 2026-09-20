import { badgeColorClasses } from '@/lib/colorClasses';
import { tipoEventoMeta, type EventoCalendario } from '@/data/calendarioMock';

export function EventoAgendaRow({
  evento,
  alcanceLabel,
  onAbrir,
}: {
  evento: EventoCalendario;
  alcanceLabel: string;
  onAbrir: () => void;
}) {
  const meta = tipoEventoMeta[evento.tipo];
  const Icon = meta.icon;
  const c = badgeColorClasses[meta.color];

  return (
    <button
      type="button"
      onClick={onAbrir}
      className="flex w-full items-center gap-3 rounded-2xl border border-slate-200 bg-white p-3 text-left shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md dark:border-slate-700 dark:bg-slate-900"
    >
      <div className={`grid h-9 w-9 shrink-0 place-items-center rounded-xl ${c.bg} ${c.text}`}>
        <Icon className="h-4 w-4" />
      </div>
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-bold text-slate-900 dark:text-white">{evento.titulo}</p>
        <p className="truncate text-xs text-slate-500 dark:text-slate-400">
          {meta.label}
          {evento.hora ? ` · ${evento.hora}` : ''} · {alcanceLabel}
        </p>
      </div>
    </button>
  );
}

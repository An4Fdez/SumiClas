import { Check } from 'lucide-react';
import { badgeColorClasses } from '@/lib/colorClasses';
import { formatFechaRelativa } from '@/lib/date';
import { tipoObjetoMeta, type ObjetoPerdido } from '@/data/objetosPerdidosMock';

export function ObjetoCard({
  objeto,
  claseNombre,
  recuperado,
  onAbrir,
  onToggleRecuperado,
}: {
  objeto: ObjetoPerdido;
  claseNombre?: string;
  recuperado: boolean;
  onAbrir: () => void;
  onToggleRecuperado: () => void;
}) {
  const meta = tipoObjetoMeta[objeto.tipo];
  const c = badgeColorClasses[meta.color];
  const alcanceLabel = objeto.alcance === 'colegio' ? 'Colegio' : (claseNombre ?? 'Clase');

  return (
    <div
      className={`relative w-full rounded-2xl border border-slate-200 bg-white shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md dark:border-slate-700 dark:bg-slate-900 ${
        recuperado ? 'opacity-60' : ''
      }`}
    >
      <button
        type="button"
        onClick={(e) => {
          e.stopPropagation();
          onToggleRecuperado();
        }}
        aria-label={recuperado ? 'Marcar como no recuperado' : 'Marcar como recuperado'}
        title={recuperado ? 'Marcar como no recuperado' : 'Marcar como recuperado'}
        className={`absolute right-3 top-3 z-10 grid h-6 w-6 place-items-center rounded-full border transition-colors ${
          recuperado
            ? 'border-mint-500 bg-mint-500 text-white'
            : 'border-slate-300 hover:bg-slate-100 dark:border-slate-600 dark:hover:bg-slate-800'
        }`}
      >
        {recuperado && <Check className="h-3.5 w-3.5" />}
      </button>

      <button type="button" onClick={onAbrir} className="flex w-full items-start gap-3 p-4 pr-11 text-left">
        {objeto.fotoUrl && (
          <img
            src={objeto.fotoUrl}
            alt=""
            className="h-16 w-16 shrink-0 rounded-xl object-cover"
          />
        )}
        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center gap-2">
            <span className={`rounded-full px-2.5 py-0.5 text-[11px] font-bold ${c.bg} ${c.text}`}>{meta.label}</span>
            <span className="rounded-full bg-slate-100 px-2.5 py-0.5 text-[11px] font-bold text-slate-600 dark:bg-slate-800 dark:text-slate-300">
              {alcanceLabel}
            </span>
            {recuperado && (
              <span className="rounded-full bg-slate-200 px-2.5 py-0.5 text-[11px] font-bold text-slate-600 dark:bg-slate-700 dark:text-slate-300">
                Recuperado
              </span>
            )}
          </div>

          <p className="mt-2 text-sm text-slate-700 dark:text-slate-300">{objeto.descripcion}</p>

          <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-xs text-slate-500 dark:text-slate-400">
            <span>
              {objeto.autorNombre} · {objeto.autorRol}
            </span>
            <span>{formatFechaRelativa(objeto.fecha)}</span>
          </div>
        </div>
      </button>
    </div>
  );
}

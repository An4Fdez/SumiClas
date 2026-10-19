import { Check } from 'lucide-react';
import { badgeColorClasses } from '@/lib/colorClasses';
import { formatRelativeDate } from '@/lib/date';
import { itemStatusMeta } from '@/lib/itemStatusMeta';
import { roleLabel } from '@/lib/roles';
import type { LostItem } from '@/modules/lostItems/domain/LostItem';

export function ItemCard({
  item,
  className,
  recovered,
  canToggleRecovered,
  onOpen,
  onToggleRecovered,
}: {
  item: LostItem;
  className?: string;
  recovered: boolean;
  canToggleRecovered: boolean;
  onOpen: () => void;
  onToggleRecovered: () => void;
}) {
  const meta = itemStatusMeta[item.status];
  const c = badgeColorClasses[meta.color];
  const scopeLabel = item.scope === 'school' ? 'Colegio' : (className ?? 'Clase');

  return (
    <div
      className={`relative w-full rounded-2xl border border-slate-200 bg-white shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md dark:border-slate-700 dark:bg-slate-900 ${
        recovered ? 'opacity-60' : ''
      }`}
    >
      {!canToggleRecovered && recovered && (
        <span
          aria-label="Recuperado"
          title="Recuperado"
          className="absolute right-3 top-3 z-10 grid h-6 w-6 place-items-center rounded-full border border-mint-500 bg-mint-500 text-white"
        >
          <Check className="h-3.5 w-3.5" />
        </span>
      )}

      {canToggleRecovered && (
        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            onToggleRecovered();
          }}
          aria-label={recovered ? 'Marcar como no recuperado' : 'Marcar como recuperado'}
          title={recovered ? 'Marcar como no recuperado' : 'Marcar como recuperado'}
          className={`absolute right-3 top-3 z-10 grid h-6 w-6 place-items-center rounded-full border transition-colors ${
            recovered
              ? 'border-mint-500 bg-mint-500 text-white'
              : 'border-slate-300 hover:bg-slate-100 dark:border-slate-600 dark:hover:bg-slate-800'
          }`}
        >
          {recovered && <Check className="h-3.5 w-3.5" />}
        </button>
      )}

      <button type="button" onClick={onOpen} className="flex w-full items-start gap-3 p-4 pr-11 text-left">
        {item.photoUrl && (
          <img
            src={item.photoUrl}
            alt=""
            className="h-16 w-16 shrink-0 rounded-xl object-cover"
          />
        )}
        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center gap-2">
            <span className={`rounded-full px-2.5 py-0.5 text-[11px] font-bold ${c.bg} ${c.text}`}>{meta.label}</span>
            <span className="rounded-full bg-slate-100 px-2.5 py-0.5 text-[11px] font-bold text-slate-600 dark:bg-slate-800 dark:text-slate-300">
              {scopeLabel}
            </span>
            {recovered && (
              <span className="rounded-full bg-slate-200 px-2.5 py-0.5 text-[11px] font-bold text-slate-600 dark:bg-slate-700 dark:text-slate-300">
                Recuperado
              </span>
            )}
          </div>

          <p className="mt-2 text-sm text-slate-700 dark:text-slate-300">{item.description}</p>

          <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-xs text-slate-500 dark:text-slate-400">
            <span>{[item.authorName, roleLabel(item.authorRole)].filter(Boolean).join(' · ')}</span>
            <span>{formatRelativeDate(item.createdAt)}</span>
          </div>
        </div>
      </button>
    </div>
  );
}

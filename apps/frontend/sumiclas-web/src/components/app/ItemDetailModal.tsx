import { useRef, type MouseEvent } from 'react';
import { Check, X } from 'lucide-react';
import { badgeColorClasses } from '@/lib/colorClasses';
import { formatRelativeDate } from '@/lib/date';
import { itemStatusMeta } from '@/lib/itemStatusMeta';
import { roleLabel } from '@/lib/roles';
import type { LostItem } from '@/modules/lostItems/domain/ILostItemRepository';

export function ItemDetailModal({
  item,
  className,
  recovered,
  onToggleRecovered,
  onClose,
}: {
  item: LostItem;
  className?: string;
  recovered: boolean;
  onToggleRecovered: () => void;
  onClose: () => void;
}) {
  const meta = itemStatusMeta[item.status];
  const c = badgeColorClasses[meta.color];
  const scopeLabel = item.scope === 'school' ? 'Colegio' : (className ?? 'Clase');
  const mouseDownOnBackdrop = useRef(false);

  return (
    <div
      className="fixed inset-0 z-50 flex items-end justify-center bg-slate-900/40 p-0 backdrop-blur-sm sm:items-center sm:p-4"
      onMouseDown={(e: MouseEvent) => {
        mouseDownOnBackdrop.current = e.target === e.currentTarget;
      }}
      onClick={(e: MouseEvent) => {
        if (mouseDownOnBackdrop.current && e.target === e.currentTarget) onClose();
      }}
    >
      <div
        className="max-h-[85vh] w-full max-w-lg overflow-y-auto rounded-t-3xl border border-slate-200 bg-white p-6 shadow-xl dark:border-slate-700 dark:bg-slate-900 sm:rounded-3xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-start justify-between gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <span className={`rounded-full px-2.5 py-0.5 text-[11px] font-bold ${c.bg} ${c.text}`}>{meta.label}</span>
            <span className="rounded-full bg-slate-100 px-2.5 py-0.5 text-[11px] font-bold text-slate-600 dark:bg-slate-800 dark:text-slate-300">
              {scopeLabel}
            </span>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        {item.photoUrl && (
          <img src={item.photoUrl} alt="" className="mt-4 max-h-72 w-full rounded-2xl object-cover" />
        )}

        <p className="mt-4 whitespace-pre-line text-sm leading-relaxed text-slate-700 dark:text-slate-300">
          {item.description}
        </p>
        <p className="mt-2 text-xs text-slate-500 dark:text-slate-400">
          {[item.authorName, roleLabel(item.authorRole), formatRelativeDate(item.createdAt)].filter(Boolean).join(' · ')}
        </p>

        <button
          type="button"
          onClick={onToggleRecovered}
          className={`mt-4 inline-flex items-center gap-1.5 rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
            recovered
              ? 'border-mint-500 bg-mint-50 text-mint-700 dark:border-mint-500 dark:bg-mint-900/20 dark:text-mint-300'
              : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
          }`}
        >
          <Check className="h-4 w-4" />
          {recovered ? 'Recuperado' : 'Marcar como recuperado'}
        </button>
      </div>
    </div>
  );
}

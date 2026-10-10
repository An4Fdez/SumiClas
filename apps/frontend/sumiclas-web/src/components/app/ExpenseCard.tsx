import { ArrowUpRight, Check } from 'lucide-react';
import { formatRelativeDate } from '@/lib/date';
import { formatEuros } from '@/lib/money';
import { roleLabel } from '@/lib/roles';
import type { ClassExpense } from '@/modules/classExpenses/domain/ClassExpense';

export function ExpenseCard({
  expense,
  iParticipate,
  iHavePaid,
  paidCount,
  amountPerFamily,
  onOpen,
  onTogglePaid,
}: {
  expense: ClassExpense;
  iParticipate: boolean;
  iHavePaid: boolean;
  paidCount: number;
  amountPerFamily: number;
  onOpen: () => void;
  onTogglePaid: () => void;
}) {
  return (
    <div className="relative w-full rounded-2xl border border-slate-200 bg-white shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md dark:border-slate-700 dark:bg-slate-900">
      {iParticipate && (
        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            onTogglePaid();
          }}
          aria-label={iHavePaid ? 'Marcar como pendiente' : 'Marcar como pagado'}
          title={iHavePaid ? 'Marcar como pendiente' : 'Marcar como pagado'}
          className={`absolute right-3 top-3 z-10 grid h-6 w-6 place-items-center rounded-full border transition-colors ${
            iHavePaid
              ? 'border-mint-500 bg-mint-500 text-white'
              : 'border-slate-300 hover:bg-slate-100 dark:border-slate-600 dark:hover:bg-slate-800'
          }`}
        >
          {iHavePaid && <Check className="h-3.5 w-3.5" />}
        </button>
      )}

      <button type="button" onClick={onOpen} className="w-full p-4 pr-11 text-left">
        <h3 className="font-display text-base font-bold text-slate-900 dark:text-white">{expense.item}</h3>
        <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">
          {paidCount}/{expense.participants.length} han pagado · {formatEuros(amountPerFamily)} por familia
        </p>

        <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-xs text-slate-500 dark:text-slate-400">
          <span>{[expense.authorName, roleLabel(expense.authorRole)].filter(Boolean).join(' · ')}</span>
          <span>{formatRelativeDate(expense.createdAt)}</span>
        </div>

        {expense.relatedEvent && (
          <span className="mt-3 inline-flex items-center gap-1 text-xs font-semibold text-violet-600 dark:text-violet-300">
            <ArrowUpRight className="h-3.5 w-3.5" />
            Ver evento: {expense.relatedEvent.title}
          </span>
        )}
      </button>
    </div>
  );
}

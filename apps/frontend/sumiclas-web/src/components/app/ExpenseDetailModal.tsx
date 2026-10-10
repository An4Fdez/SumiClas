import { useRef, type MouseEvent } from 'react';
import { ArrowUpRight, Check, X } from 'lucide-react';
import { Link } from 'react-router-dom';
import { formatRelativeDate } from '@/lib/date';
import { formatEuros } from '@/lib/money';
import { roleLabel } from '@/lib/roles';
import type { ClassExpense } from '@/modules/classExpenses/domain/ClassExpense';

export type ParticipantStatus = {
  id: string;
  name: string;
  paid: boolean;
  isMe: boolean;
};

export function ExpenseDetailModal({
  expense,
  amountPerFamily,
  participants,
  onTogglePaid,
  onClose,
}: {
  expense: ClassExpense;
  amountPerFamily: number;
  participants: ParticipantStatus[];
  onTogglePaid: () => void;
  onClose: () => void;
}) {
  const mouseDownOnBackdrop = useRef(false);
  const me = participants.find((p) => p.isMe);

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
          <h2 className="font-display text-xl font-extrabold text-slate-900 dark:text-white">{expense.item}</h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>
        <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
          {[expense.authorName, roleLabel(expense.authorRole), formatRelativeDate(expense.createdAt)]
            .filter(Boolean)
            .join(' · ')}
        </p>

        <div className="mt-4 space-y-1 text-sm text-slate-700 dark:text-slate-300">
          <p>Importe total: {formatEuros(expense.totalAmount)}</p>
          <p>Por familia: {formatEuros(amountPerFamily)}</p>
        </div>

        {expense.relatedEvent && (
          <Link
            to={`/calendar?event=${expense.relatedEvent.id}`}
            className="mt-4 inline-flex items-center gap-1.5 rounded-xl border border-violet-200 px-3 py-2 text-sm font-semibold text-violet-700 transition-colors hover:bg-violet-50 dark:border-violet-800 dark:text-violet-300 dark:hover:bg-violet-900/20"
          >
            <ArrowUpRight className="h-4 w-4" />
            Ver evento: {expense.relatedEvent.title}
          </Link>
        )}

        {me && (
          <button
            type="button"
            onClick={onTogglePaid}
            className={`mt-4 inline-flex items-center gap-1.5 rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
              me.paid
                ? 'border-mint-500 bg-mint-50 text-mint-700 dark:border-mint-500 dark:bg-mint-900/20 dark:text-mint-300'
                : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
            }`}
          >
            <Check className="h-4 w-4" />
            {me.paid ? 'Has pagado' : 'Marcar como pagado'}
          </button>
        )}

        <div className="mt-6">
          <h3 className="text-sm font-bold text-slate-900 dark:text-white">Participantes</h3>
          <div className="mt-3 space-y-2">
            {participants.map((p) => (
              <div
                key={p.id}
                className="flex items-center justify-between rounded-xl bg-slate-50 px-3 py-2 dark:bg-slate-800"
              >
                <span className="text-sm font-semibold text-slate-700 dark:text-slate-300">
                  {p.isMe ? 'Tú' : p.name}
                </span>
                <span
                  className={`text-xs font-bold ${
                    p.paid ? 'text-mint-600 dark:text-mint-400' : 'text-slate-400 dark:text-slate-500'
                  }`}
                >
                  {p.paid ? 'Pagado' : 'Pendiente'}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

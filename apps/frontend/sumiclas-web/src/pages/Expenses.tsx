import { useMemo, useState } from 'react';
import { Plus, Wallet } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { ExpenseCard } from '@/components/app/ExpenseCard';
import { ExpenseDetailModal, type ParticipantStatus } from '@/components/app/ExpenseDetailModal';
import { ExpenseForm, type NewExpense } from '@/components/app/ExpenseForm';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses } from '@/lib/colorClasses';
import { getGuardianInfo } from '@/lib/auth';
import { roleMeta } from '@/lib/roles';
import { otherGuardiansByChild } from '@/data/appMock';
import { calendarEventsMock } from '@/data/calendarMock';
import { classExpensesMock, type ClassExpense } from '@/data/expensesMock';

type StatusFilter = 'pending' | 'paid' | null;

function participantName(id: string, childId: string): string {
  if (id === 'me') return getGuardianInfo()?.name ?? 'Tú';
  return otherGuardiansByChild[childId]?.find((t) => t.id === id)?.name ?? id;
}

export default function Expenses() {
  const { children, currentChild, currentRole, otherGuardians } = useChildContext();

  const [createdExpenses, setCreatedExpenses] = useState<ClassExpense[]>([]);
  const [toggledPayments, setToggledPayments] = useState<Set<string>>(new Set());
  const [statusFilter, setStatusFilter] = useState<StatusFilter>(null);
  const [openExpense, setOpenExpense] = useState<ClassExpense | null>(null);
  const [formOpen, setFormOpen] = useState(false);

  const iHavePaid = (expense: ClassExpense) => {
    const base = expense.paidBy.includes('me');
    return toggledPayments.has(expense.id) ? !base : base;
  };

  const countPaid = (expense: ClassExpense) => {
    const paid = new Set(expense.paidBy);
    if (expense.participantIds.includes('me')) {
      if (iHavePaid(expense)) paid.add('me');
      else paid.delete('me');
    }
    return paid.size;
  };

  const visibleExpenses = useMemo(() => {
    const myChildIds = new Set(children.map((c) => c.id));
    return [...classExpensesMock, ...createdExpenses]
      .filter((e) => myChildIds.has(e.childId))
      .filter((e) => {
        if (statusFilter === 'pending') return !iHavePaid(e);
        if (statusFilter === 'paid') return iHavePaid(e);
        return true;
      })
      .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [children, createdExpenses, statusFilter, toggledPayments]);

  const togglePaid = (id: string) =>
    setToggledPayments((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  const canCreate = currentRole === 'classRepresentative' && !!currentChild;

  const handleCreateExpense = (data: NewExpense) => {
    if (!currentChild) return;
    const event = data.eventId ? calendarEventsMock.find((ev) => ev.id === data.eventId) : undefined;
    const created: ClassExpense = {
      id: `local-${Date.now()}`,
      childId: currentChild.id,
      item: data.item,
      totalAmount: data.totalAmount,
      participantIds: data.participantIds,
      paidBy: [],
      relatedEvent: event ? { id: event.id, title: event.title } : undefined,
      date: new Date().toISOString(),
      authorName: getGuardianInfo()?.name ?? 'Tú',
      authorRole: roleMeta.classRepresentative.label,
    };
    setCreatedExpenses((prev) => [...prev, created]);
    setFormOpen(false);
  };

  const participantsForExpense = (expense: ClassExpense): ParticipantStatus[] =>
    expense.participantIds.map((id) => ({
      id,
      name: participantName(id, expense.childId),
      paid: id === 'me' ? iHavePaid(expense) : expense.paidBy.includes(id),
      isMe: id === 'me',
    }));

  return (
    <div>
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div
            className={`grid h-12 w-12 place-items-center rounded-xl ${badgeColorClasses.sun.bg} ${badgeColorClasses.sun.text}`}
          >
            <Wallet className="h-6 w-6" />
          </div>
          <h1 className="mt-4 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
            Gastos de clase
          </h1>
          <p className="mt-2 truncate text-slate-600 dark:text-slate-400">
            Llevad la cuenta de los pagos entre todos: regalos, material, excursiones… sin mover ni un euro dentro de la app.
          </p>
        </div>

        {canCreate && (
          <button
            type="button"
            onClick={() => setFormOpen(true)}
            className="inline-flex shrink-0 items-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-3.5 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25"
          >
            <Plus className="h-4 w-4" />
            Nuevo
          </button>
        )}
      </div>

      <div className="mt-6 flex flex-wrap items-center gap-2">
        <button
          type="button"
          onClick={() => setStatusFilter((v) => (v === 'pending' ? null : 'pending'))}
          className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
            statusFilter === 'pending'
              ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
              : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
          }`}
        >
          Pendientes
        </button>
        <button
          type="button"
          onClick={() => setStatusFilter((v) => (v === 'paid' ? null : 'paid'))}
          className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
            statusFilter === 'paid'
              ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
              : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
          }`}
        >
          Pagados
        </button>
      </div>

      {visibleExpenses.length === 0 ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          No hay gastos de clase por ahora.
        </div>
      ) : (
        <div className="mt-6 space-y-3">
          {visibleExpenses.map((expense) => (
            <Reveal key={expense.id}>
              <ExpenseCard
                expense={expense}
                iParticipate={expense.participantIds.includes('me')}
                iHavePaid={iHavePaid(expense)}
                paidCount={countPaid(expense)}
                amountPerFamily={expense.totalAmount / expense.participantIds.length}
                onOpen={() => setOpenExpense(expense)}
                onTogglePaid={() => togglePaid(expense.id)}
              />
            </Reveal>
          ))}
        </div>
      )}

      {openExpense && (
        <ExpenseDetailModal
          expense={openExpense}
          amountPerFamily={openExpense.totalAmount / openExpense.participantIds.length}
          participants={participantsForExpense(openExpense)}
          onTogglePaid={() => togglePaid(openExpense.id)}
          onClose={() => setOpenExpense(null)}
        />
      )}

      {formOpen && currentChild && (
        <ExpenseForm
          className={currentChild.className}
          events={calendarEventsMock}
          availableParticipants={[
            { id: 'me', name: getGuardianInfo()?.name ?? 'Tú' },
            ...otherGuardians.map((t) => ({ id: t.id, name: t.name })),
          ]}
          onSubmit={handleCreateExpense}
          onClose={() => setFormOpen(false)}
        />
      )}
    </div>
  );
}

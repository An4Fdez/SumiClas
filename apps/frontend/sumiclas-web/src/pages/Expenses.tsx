import { useMemo, useState } from 'react';
import { Plus, Wallet } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { ExpenseCard } from '@/components/app/ExpenseCard';
import { ExpenseDetailModal, type ParticipantStatus } from '@/components/app/ExpenseDetailModal';
import { ExpenseForm, type NewExpense } from '@/components/app/ExpenseForm';
import { useChildContext } from '@/hooks/useChildContext';
import { useClassExpenseRepository } from '@/hooks/useClassExpenseRepository';
import { useClassExpenses } from '@/hooks/useClassExpenses';
import { useClassMembers } from '@/hooks/useClassMembers';
import { useUpcomingEventOptions } from '@/hooks/useUpcomingEventOptions';
import { useWizardIds } from '@/hooks/useWizardIds';
import { getApiErrorStatus, UserFacingError } from '@/lib/apiError';
import { badgeColorClasses } from '@/lib/colorClasses';
import type { ClassExpense } from '@/modules/classExpenses/domain/ClassExpense';

type StatusFilter = 'pending' | 'paid' | null;

const TRANSIENT_CLIENT_ERRORS = [408, 429];
const NO_CLASS_ERROR = 'Este alumno todavía no está en ninguna clase.';
const SECURE_CONTEXT_ERROR = 'Tu navegador no permite crear entradas fuera de una conexión segura (HTTPS).';

const filterButtonClasses = (active: boolean) =>
  `rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
    active
      ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
      : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
  }`;

export default function Expenses() {
  const { currentChild, currentRole, currentUserId } = useChildContext();
  const classId = currentChild?.classId;
  const repository = useClassExpenseRepository();
  const ids = useWizardIds();
  const { state, reload, loadMore, loadingMore, loadMoreError, setPaid, paymentError } = useClassExpenses(classId);

  const [statusFilter, setStatusFilter] = useState<StatusFilter>(null);
  const [openExpenseId, setOpenExpenseId] = useState<string | null>(null);
  const [formForClass, setFormForClass] = useState<string | null>(null);
  const [editingExpenseId, setEditingExpenseId] = useState<string | null>(null);

  const formOpen = !!classId && formForClass === classId;
  const members = useClassMembers(formOpen ? classId : undefined);
  const loaded = useMemo(() => (state.status === 'ready' ? state.items : []), [state]);
  const editingExpense = loaded.find((e) => e.id === editingExpenseId) ?? null;
  const eventOptions = useUpcomingEventOptions(formOpen || editingExpense ? classId : undefined);
  const editEventOptions = useMemo(() => {
    const current = editingExpense?.relatedEvent;
    if (!current || eventOptions?.some((ev) => ev.id === current.id)) return eventOptions;
    return [current, ...(eventOptions ?? [])];
  }, [editingExpense, eventOptions]);
  const hasMore = state.status === 'ready' && !!state.nextCursor;
  const openExpense = loaded.find((e) => e.id === openExpenseId) ?? null;

  const myShare = (expense: ClassExpense) => expense.participants.find((p) => p.guardianId === currentUserId);
  const iHavePaid = (expense: ClassExpense) => myShare(expense)?.hasPaid ?? false;
  const paidCount = (expense: ClassExpense) => expense.participants.filter((p) => p.hasPaid).length;
  const amountPerFamily = (expense: ClassExpense) => expense.totalAmount / Math.max(expense.participants.length, 1);

  const visibleExpenses = useMemo(
    () =>
      loaded.filter((expense) => {
        const paid = expense.participants.find((p) => p.guardianId === currentUserId)?.hasPaid ?? false;
        if (statusFilter === 'pending') return !paid;
        if (statusFilter === 'paid') return paid;
        return true;
      }),
    [loaded, statusFilter, currentUserId],
  );

  const canCreate = currentRole === 'classRepresentative' && !!classId;

  const handleTogglePaid = (expense: ClassExpense) => {
    if (!currentUserId || !myShare(expense)) return;
    setPaid(expense.id, currentUserId, !iHavePaid(expense));
  };

  const takeId = (key: string) => {
    try {
      return ids.take(key);
    } catch {
      throw new UserFacingError(SECURE_CONTEXT_ERROR);
    }
  };

  const settleId = (key: string, error: unknown) => {
    const status = getApiErrorStatus(error);
    if (status !== undefined && status < 500 && !TRANSIENT_CLIENT_ERRORS.includes(status)) ids.consume(key);
    return status;
  };

  const handleUpdateExpense = async (data: NewExpense) => {
    if (!editingExpense) return;
    await repository.update(editingExpense.id, {
      item: data.item,
      totalAmount: data.totalAmount,
      relatedEventId: data.eventId ?? undefined,
    });
    setEditingExpenseId(null);
    reload();
  };

  const handleDeleteExpense = async (expense: ClassExpense) => {
    await repository.remove(expense.id);
    setOpenExpenseId(null);
    reload();
  };

  const handleCreateExpense = async (data: NewExpense) => {
    if (!classId) throw new UserFacingError(NO_CLASS_ERROR);
    const key = `expense:${classId}:${JSON.stringify(data)}`;
    const id = takeId(key);
    try {
      await repository.create({
        id,
        classId,
        item: data.item,
        totalAmount: data.totalAmount,
        participantIds: data.participantIds,
        relatedEventId: data.eventId ?? undefined,
      });
    } catch (error) {
      if (settleId(key, error) === 409) {
        reload();
        throw new UserFacingError('Es posible que el gasto ya se haya creado. Revisa la lista antes de crear otro.');
      }
      throw error;
    }
    ids.consume(key);
    setFormForClass(null);
    reload();
  };

  const participantsForExpense = (expense: ClassExpense): ParticipantStatus[] =>
    expense.participants.map((p) => ({
      id: p.guardianId,
      name: p.fullName,
      paid: p.hasPaid,
      isMe: p.guardianId === currentUserId,
    }));

  const emptyMessage = hasMore
    ? 'Ningún gasto de los cargados coincide con los filtros. Pulsa «Cargar más» para buscar en más.'
    : loaded.length > 0
      ? 'Ningún gasto coincide con los filtros.'
      : 'No hay gastos de clase por ahora.';

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
            onClick={() => setFormForClass(classId ?? null)}
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
          className={filterButtonClasses(statusFilter === 'pending')}
        >
          Pendientes
        </button>
        <button
          type="button"
          onClick={() => setStatusFilter((v) => (v === 'paid' ? null : 'paid'))}
          className={filterButtonClasses(statusFilter === 'paid')}
        >
          Pagados
        </button>
      </div>

      {paymentError && (
        <p role="alert" className="mt-4 text-sm font-semibold text-rose-600 dark:text-rose-400">
          {paymentError}
        </p>
      )}

      {!classId ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          Tu hijo/a todavía no está en ninguna clase, así que no hay gastos que mostrar.
        </div>
      ) : state.status === 'loading' || state.status === 'idle' ? (
        <p className="mt-8 text-sm text-slate-500 dark:text-slate-400">Cargando los gastos…</p>
      ) : state.status === 'error' ? (
        <div role="alert" className="mt-8 rounded-2xl border border-rose-200 bg-rose-50/60 p-4 dark:border-rose-700/40 dark:bg-rose-900/15">
          <p className="text-sm text-slate-700 dark:text-slate-300">{state.message}</p>
          <button
            type="button"
            onClick={reload}
            className="mt-3 text-sm font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
          >
            Reintentar
          </button>
        </div>
      ) : visibleExpenses.length === 0 ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          {emptyMessage}
        </div>
      ) : (
        <div className="mt-6 space-y-3">
          {visibleExpenses.map((expense) => (
            <Reveal key={expense.id}>
              <ExpenseCard
                expense={expense}
                iParticipate={!!myShare(expense)}
                iHavePaid={iHavePaid(expense)}
                paidCount={paidCount(expense)}
                amountPerFamily={amountPerFamily(expense)}
                onOpen={() => setOpenExpenseId(expense.id)}
                onTogglePaid={() => handleTogglePaid(expense)}
              />
            </Reveal>
          ))}
        </div>
      )}

      {hasMore && (
        <div className="mt-6 text-center">
          <button
            type="button"
            onClick={loadMore}
            disabled={loadingMore}
            className="rounded-full bg-slate-100 px-5 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-200 disabled:opacity-60 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700"
          >
            {loadingMore ? 'Cargando…' : 'Cargar más'}
          </button>
          {loadMoreError && (
            <p role="alert" className="mt-2 text-xs font-semibold text-rose-600 dark:text-rose-400">
              {loadMoreError}
            </p>
          )}
        </div>
      )}

      {openExpense && (
        <ExpenseDetailModal
          expense={openExpense}
          amountPerFamily={amountPerFamily(openExpense)}
          participants={participantsForExpense(openExpense)}
          onTogglePaid={() => handleTogglePaid(openExpense)}
          onClose={() => setOpenExpenseId(null)}
          onEdit={() => {
            setEditingExpenseId(openExpense.id);
            setOpenExpenseId(null);
          }}
          onDelete={() => handleDeleteExpense(openExpense)}
        />
      )}

      {editingExpense && currentChild && (
        <ExpenseForm
          className={currentChild.className}
          events={editEventOptions}
          expense={editingExpense}
          onSubmit={handleUpdateExpense}
          onClose={() => setEditingExpenseId(null)}
        />
      )}

      {formOpen && currentChild && (
        <ExpenseForm
          className={currentChild.className}
          events={eventOptions}
          members={members.state}
          onRetryMembers={members.reload}
          onSubmit={handleCreateExpense}
          onClose={() => setFormForClass(null)}
        />
      )}
    </div>
  );
}

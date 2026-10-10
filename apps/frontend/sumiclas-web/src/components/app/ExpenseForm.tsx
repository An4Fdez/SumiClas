import { useRef, useState, type FormEvent, type MouseEvent } from 'react';
import { X } from 'lucide-react';
import { ParticipantsDropdown } from '@/components/app/ParticipantsDropdown';
import { SelectDropdown } from '@/components/app/SelectDropdown';
import { getApiErrorMessage } from '@/lib/apiError';
import { centsToEuros, formatEuros, MAX_TOTAL_CENTS, parseEuroAmount } from '@/lib/money';
import type { ClassMember } from '@/modules/users/domain/ClassMember';

export type NewExpense = {
  eventId: string | null;
  item: string;
  totalAmount: number;
  participantIds: string[];
};

export type ExpenseFormMembers =
  | { status: 'idle' | 'loading' }
  | { status: 'ready'; members: ClassMember[] }
  | { status: 'error'; message: string };

const inputClasses =
  'mt-1.5 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white';

const TOTAL_TOO_HIGH = 'El importe total supera el máximo permitido.';

export function ExpenseForm({
  className,
  events,
  members,
  onRetryMembers,
  onSubmit,
  onClose,
}: {
  className: string;
  events: { id: string; title: string }[] | undefined;
  members: ExpenseFormMembers;
  onRetryMembers: () => void;
  onSubmit: (data: NewExpense) => Promise<void>;
  onClose: () => void;
}) {
  const [eventId, setEventId] = useState('');
  const [item, setItem] = useState('');
  const [amountPerFamily, setAmountPerFamily] = useState('');
  const [selected, setSelected] = useState<Set<string> | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const mouseDownOnBackdrop = useRef(false);

  const available = members.status === 'ready' ? members.members.map((m) => ({ id: m.userId, name: m.fullName })) : [];
  const participantIds = selected ?? new Set(available.map((p) => p.id));

  const parsed = amountPerFamily.trim() === '' ? null : parseEuroAmount(amountPerFamily);
  const amountError = parsed && 'error' in parsed ? parsed.error : null;
  const perFamilyCents = parsed && 'cents' in parsed ? parsed.cents : 0;
  const totalCents = perFamilyCents * participantIds.size;
  const totalError = totalCents > MAX_TOTAL_CENTS ? TOTAL_TOO_HIGH : null;

  const canSubmit =
    !submitting &&
    members.status === 'ready' &&
    item.trim().length > 0 &&
    perFamilyCents > 0 &&
    participantIds.size > 0 &&
    !totalError;

  const handleEventChange = (id: string) => {
    setEventId(id);
    const event = events?.find((ev) => ev.id === id);
    if (event) setItem(event.title.slice(0, 200));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!canSubmit) return;
    setSubmitting(true);
    setSubmitError(null);
    try {
      await onSubmit({
        eventId: eventId || null,
        item: item.trim(),
        totalAmount: centsToEuros(totalCents),
        participantIds: [...participantIds],
      });
    } catch (error) {
      setSubmitError(getApiErrorMessage(error));
    } finally {
      setSubmitting(false);
    }
  };

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
      <form
        onSubmit={handleSubmit}
        onClick={(e) => e.stopPropagation()}
        className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-t-3xl border border-slate-200 bg-white p-6 shadow-xl dark:border-slate-700 dark:bg-slate-900 sm:rounded-3xl"
      >
        <div className="flex items-center justify-between">
          <h2 className="font-display text-xl font-extrabold text-slate-900 dark:text-white">Nuevo gasto</h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">Se publicará para {className}.</p>

        {events && events.length > 0 && (
          <div className="mt-5">
            <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">
              Evento relacionado (opcional)
            </span>
            <div className="mt-1.5">
              <SelectDropdown
                value={eventId}
                placeholder="Ninguno"
                options={[{ value: '', label: 'Ninguno' }, ...events.map((ev) => ({ value: ev.id, label: ev.title }))]}
                onChange={handleEventChange}
              />
            </div>
          </div>
        )}

        <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
          Concepto
          <input
            value={item}
            onChange={(e) => setItem(e.target.value)}
            maxLength={200}
            required
            className={inputClasses}
            placeholder="Ej. Regalo de fin de curso"
          />
        </label>

        <div className="mt-4">
          <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">Participantes</span>
          <div className="mt-1.5">
            {members.status === 'ready' ? (
              available.length > 0 ? (
                <ParticipantsDropdown availableParticipants={available} selected={participantIds} onChange={setSelected} />
              ) : (
                <p className="text-sm text-slate-500 dark:text-slate-400">Esta clase todavía no tiene familias.</p>
              )
            ) : members.status === 'error' ? (
              <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50/60 p-3 dark:border-rose-700/40 dark:bg-rose-900/15">
                <p className="text-sm text-slate-700 dark:text-slate-300">{members.message}</p>
                <button
                  type="button"
                  onClick={onRetryMembers}
                  className="mt-2 text-sm font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
                >
                  Reintentar
                </button>
              </div>
            ) : (
              <p className="text-sm text-slate-500 dark:text-slate-400">Cargando las familias de la clase…</p>
            )}
          </div>
        </div>

        <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
          Importe por familia (€)
          <input
            type="text"
            inputMode="decimal"
            value={amountPerFamily}
            onChange={(e) => setAmountPerFamily(e.target.value)}
            required
            aria-invalid={!!amountError}
            className={inputClasses}
            placeholder="Ej. 5 o 5,50"
          />
        </label>
        {amountError ? (
          <p role="alert" className="mt-1.5 text-xs font-semibold text-rose-600 dark:text-rose-400">
            {amountError}
          </p>
        ) : totalError ? (
          <p role="alert" className="mt-1.5 text-xs font-semibold text-rose-600 dark:text-rose-400">
            {totalError}
          </p>
        ) : (
          <p className="mt-1.5 text-xs text-slate-500 dark:text-slate-400">
            Total: {formatEuros(centsToEuros(totalCents))} entre {participantIds.size}{' '}
            {participantIds.size === 1 ? 'familia' : 'familias'}
          </p>
        )}

        {submitError && (
          <p role="alert" className="mt-3 text-sm font-semibold text-rose-600 dark:text-rose-400">
            {submitError}
          </p>
        )}

        <button
          type="submit"
          disabled={!canSubmit}
          className="mt-6 w-full rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 transition-opacity disabled:cursor-not-allowed disabled:opacity-50"
        >
          {submitting ? 'Publicando…' : 'Publicar gasto'}
        </button>
      </form>
    </div>
  );
}

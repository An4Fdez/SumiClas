import { useRef, useState, type FormEvent, type MouseEvent } from 'react';
import { X } from 'lucide-react';
import { ParticipantsDropdown } from '@/components/app/ParticipantsDropdown';
import { SelectDropdown } from '@/components/app/SelectDropdown';
import type { RelatedEvent } from '@/data/calendarMock';

export type NewExpense = {
  eventId: string | null;
  item: string;
  totalAmount: number;
  participantIds: string[];
};

const inputClasses =
  'mt-1.5 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white';

export function ExpenseForm({
  className,
  events,
  availableParticipants,
  onSubmit,
  onClose,
}: {
  className: string;
  events: RelatedEvent[];
  availableParticipants: { id: string; name: string }[];
  onSubmit: (data: NewExpense) => void;
  onClose: () => void;
}) {
  const [eventId, setEventId] = useState('');
  const [item, setItem] = useState('');
  const [amountPerFamily, setAmountPerFamily] = useState('');
  const [participantIds, setParticipantIds] = useState<Set<string>>(
    new Set(availableParticipants.map((p) => p.id))
  );
  const mouseDownOnBackdrop = useRef(false);

  const canSubmit =
    item.trim().length > 0 && Number(amountPerFamily) > 0 && participantIds.size > 0;
  const computedTotalAmount = (Number(amountPerFamily) || 0) * participantIds.size;

  const handleEventChange = (id: string) => {
    setEventId(id);
    const event = events.find((ev) => ev.id === id);
    if (event) setItem(event.title);
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!canSubmit) return;
    onSubmit({
      eventId: eventId || null,
      item: item.trim(),
      totalAmount: computedTotalAmount,
      participantIds: [...participantIds],
    });
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

        {events.length > 0 && (
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
            maxLength={80}
            required
            className={inputClasses}
            placeholder="Ej. Regalo de fin de curso"
          />
        </label>

        <div className="mt-4">
          <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">Participantes</span>
          <div className="mt-1.5">
            <ParticipantsDropdown
              availableParticipants={availableParticipants}
              selected={participantIds}
              onChange={setParticipantIds}
            />
          </div>
        </div>

        <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
          Importe por familia (€)
          <input
            type="number"
            min="0"
            step="0.01"
            value={amountPerFamily}
            onChange={(e) => setAmountPerFamily(e.target.value)}
            required
            className={inputClasses}
            placeholder="Ej. 5"
          />
        </label>
        <p className="mt-1.5 text-xs text-slate-500 dark:text-slate-400">
          Total: {computedTotalAmount.toFixed(2)} € entre {participantIds.size}{' '}
          {participantIds.size === 1 ? 'familia' : 'familias'}
        </p>

        <button
          type="submit"
          disabled={!canSubmit}
          className="mt-6 w-full rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 transition-opacity disabled:cursor-not-allowed disabled:opacity-50"
        >
          Publicar gasto
        </button>
      </form>
    </div>
  );
}

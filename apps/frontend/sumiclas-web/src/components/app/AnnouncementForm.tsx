import { useRef, useState, type FormEvent, type MouseEvent } from 'react';
import { X } from 'lucide-react';
import { getApiErrorMessage } from '@/lib/apiError';
import type { Announcement } from '@/modules/announcements/domain/Announcement';
import type { Priority } from '@/modules/announcements/domain/Priority';
import type { RelatedEvent } from '@/modules/announcements/domain/RelatedEvent';
import { SelectDropdown } from './SelectDropdown';

const priorityOptions: { value: Priority; label: string }[] = [
  { value: 'low', label: 'Baja' },
  { value: 'normal', label: 'Normal' },
  { value: 'high', label: 'Alta (urgente)' },
];

export function AnnouncementForm({
  className,
  events,
  announcement,
  onSubmit,
  onClose,
}: {
  className: string;
  events?: RelatedEvent[];
  announcement?: Announcement;
  onSubmit: (data: { title: string; body: string; priority: Priority; eventId: string | null }) => Promise<void>;
  onClose: () => void;
}) {
  const editing = announcement !== undefined;
  const [title, setTitle] = useState(announcement?.title ?? '');
  const [body, setBody] = useState(announcement?.body ?? '');
  const [priority, setPriority] = useState<Priority>(announcement?.priority ?? 'normal');
  const [eventId, setEventId] = useState(announcement?.relatedEvent?.id ?? '');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const mouseDownOnBackdrop = useRef(false);

  const canSubmit = title.trim().length > 0 && body.trim().length > 0 && !submitting;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!canSubmit) return;
    setSubmitting(true);
    setError(null);
    try {
      await onSubmit({ title: title.trim(), body: body.trim(), priority, eventId: eventId || null });
    } catch (err) {
      setError(getApiErrorMessage(err));
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
          <h2 className="font-display text-xl font-extrabold text-slate-900 dark:text-white">
            {editing ? 'Editar aviso' : 'Nuevo aviso'}
          </h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">
          {editing
            ? 'Al guardar, el aviso volverá a aparecer como no leído para las familias.'
            : `Se publicará para ${className}.`}
        </p>

        <label className="mt-5 block text-sm font-semibold text-slate-700 dark:text-slate-300">
          Título
          <input
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            maxLength={80}
            required
            className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white"
            placeholder="Ej. Examen de matemáticas el viernes"
          />
        </label>

        <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
          Mensaje
          <textarea
            value={body}
            onChange={(e) => setBody(e.target.value)}
            required
            rows={4}
            className="mt-1.5 w-full resize-none rounded-xl border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white"
            placeholder="Escribe el mensaje para las familias..."
          />
        </label>

        <fieldset className="mt-4">
          <legend className="text-sm font-semibold text-slate-700 dark:text-slate-300">Prioridad</legend>
          <div className="mt-1.5 flex gap-2">
            {priorityOptions.map((op) => (
              <button
                key={op.value}
                type="button"
                onClick={() => setPriority(op.value)}
                className={`flex-1 rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                  priority === op.value
                    ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/20 dark:text-brand-300'
                    : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
                }`}
              >
                {op.label}
              </button>
            ))}
          </div>
          {priority === 'high' && (
            <p className="mt-1.5 text-xs text-rose-600 dark:text-rose-400">
              Se enviará una notificación push inmediata a todas las familias.
            </p>
          )}
        </fieldset>

        {events && events.length > 0 && (
          <div className="mt-4">
            <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">
              Evento relacionado (opcional)
            </span>
            <div className="mt-1.5">
              <SelectDropdown
                value={eventId}
                placeholder="Ninguno"
                options={[{ value: '', label: 'Ninguno' }, ...events.map((ev) => ({ value: ev.id, label: ev.title }))]}
                onChange={setEventId}
              />
            </div>
          </div>
        )}

        {error && (
          <p role="alert" className="mt-4 text-sm font-semibold text-rose-600 dark:text-rose-400">
            {error}
          </p>
        )}

        <button
          type="submit"
          disabled={!canSubmit}
          className="mt-6 w-full rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 transition-opacity disabled:cursor-not-allowed disabled:opacity-50"
        >
          {submitting ? (editing ? 'Guardando…' : 'Publicando…') : editing ? 'Guardar cambios' : 'Publicar aviso'}
        </button>
      </form>
    </div>
  );
}

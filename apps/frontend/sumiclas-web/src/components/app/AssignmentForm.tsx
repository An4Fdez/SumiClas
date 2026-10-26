import { useRef, useState, type FormEvent, type KeyboardEvent, type MouseEvent } from 'react';
import { Plus, X } from 'lucide-react';
import { getApiErrorMessage } from '@/lib/apiError';
import { toDateInputValue } from '@/lib/date';
import type { Assignment } from '@/modules/assignments/domain/Assignment';
import type { Subject } from '@/modules/subjects/domain/Subject';
import { DatePicker } from './DatePicker';

export type AssignmentFormData = {
  subjectId: string;
  title: string;
  description: string;
  dueDate: string;
};

const inputClasses =
  'mt-1.5 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white';

export function AssignmentForm({
  canPublishOfficial,
  className,
  subjects,
  onCreateSubject,
  assignment,
  onSubmit,
  onClose,
}: {
  canPublishOfficial: boolean;
  className: string;
  subjects: Subject[];
  onCreateSubject: (name: string) => Promise<Subject>;
  assignment?: Assignment;
  onSubmit: (data: AssignmentFormData) => Promise<void>;
  onClose: () => void;
}) {
  const editing = assignment !== undefined;
  const [pickedSubjectId, setPickedSubjectId] = useState(assignment?.subjectId ?? '');
  const [creatingSubject, setCreatingSubject] = useState(!editing && canPublishOfficial && subjects.length === 0);
  const [newSubjectName, setNewSubjectName] = useState('');
  const [subjectBusy, setSubjectBusy] = useState(false);
  const [subjectError, setSubjectError] = useState<string | null>(null);
  const [title, setTitle] = useState(assignment?.title ?? '');
  const [description, setDescription] = useState(assignment?.description ?? '');
  const [dueDate, setDueDate] = useState(assignment ? toDateInputValue(assignment.dueDate) : '');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const mouseDownOnBackdrop = useRef(false);

  const subjectId = subjects.some((s) => s.id === pickedSubjectId) ? pickedSubjectId : (subjects[0]?.id ?? '');
  const canSubmit = subjectId.length > 0 && title.trim().length > 0 && dueDate.length > 0 && !submitting;
  const noSubjectsForFamily = !canPublishOfficial && subjects.length === 0;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!canSubmit) return;
    setSubmitting(true);
    setError(null);
    try {
      await onSubmit({ subjectId, title: title.trim(), description: description.trim(), dueDate });
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  };

  const confirmNewSubject = async () => {
    const name = newSubjectName.trim();
    if (!name || subjectBusy) return;
    setSubjectBusy(true);
    setSubjectError(null);
    try {
      const created = await onCreateSubject(name);
      setPickedSubjectId(created.id);
      setNewSubjectName('');
      setCreatingSubject(false);
    } catch (err) {
      setSubjectError(getApiErrorMessage(err));
    } finally {
      setSubjectBusy(false);
    }
  };

  const handleKeyDownNewSubject = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      void confirmNewSubject();
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
            {editing ? 'Editar tarea' : canPublishOfficial ? 'Nueva tarea' : 'Proponer tarea'}
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

        {noSubjectsForFamily ? (
          <p
            role="status"
            className="mt-4 rounded-2xl border border-dashed border-slate-300 bg-slate-50 p-5 text-sm text-slate-600 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400"
          >
            Todavía no hay asignaturas en {className}. El delegado o la delegada de la clase tiene que crearlas antes de
            que puedas proponer tareas.
          </p>
        ) : (
          <>
            <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">
              {editing
                ? 'Al guardar, la tarea volverá a quedar pendiente para todas las familias.'
                : canPublishOfficial
                  ? `Se publicará como tarea oficial para ${className}.`
                  : `Se mostrará como propuesta de familia para ${className}.`}
            </p>

            <fieldset className="mt-5">
              <legend className="text-sm font-semibold text-slate-700 dark:text-slate-300">Asignatura</legend>
              <div className="mt-1.5 flex flex-wrap gap-2">
                {subjects.map((s) => (
                  <button
                    key={s.id}
                    type="button"
                    onClick={() => setPickedSubjectId(s.id)}
                    className={`rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                      subjectId === s.id
                        ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/20 dark:text-brand-300'
                        : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
                    }`}
                  >
                    {s.label}
                  </button>
                ))}
                {canPublishOfficial && !creatingSubject && (
                  <button
                    type="button"
                    onClick={() => setCreatingSubject(true)}
                    className="inline-flex items-center gap-1 rounded-xl border border-dashed border-slate-300 px-3 py-2 text-sm font-semibold text-slate-500 transition-colors hover:bg-slate-50 dark:border-slate-600 dark:text-slate-400 dark:hover:bg-slate-800"
                  >
                    <Plus className="h-3.5 w-3.5" />
                    Añadir asignatura
                  </button>
                )}
              </div>

              {canPublishOfficial && creatingSubject && (
                <div className="mt-2 flex items-center gap-2">
                  <input
                    value={newSubjectName}
                    onChange={(e) => setNewSubjectName(e.target.value)}
                    onKeyDown={handleKeyDownNewSubject}
                    autoFocus
                    maxLength={30}
                    placeholder="Nombre de la asignatura"
                    className="flex-1 rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white"
                  />
                  <button
                    type="button"
                    onClick={() => void confirmNewSubject()}
                    disabled={!newSubjectName.trim() || subjectBusy}
                    className="rounded-xl bg-brand-600 px-3 py-2 text-sm font-semibold text-white transition-opacity disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    {subjectBusy ? 'Añadiendo…' : 'Añadir'}
                  </button>
                  {subjects.length > 0 && (
                    <button
                      type="button"
                      onClick={() => {
                        setCreatingSubject(false);
                        setNewSubjectName('');
                        setSubjectError(null);
                      }}
                      className="rounded-full p-2 text-slate-400 transition-colors hover:bg-slate-100 dark:hover:bg-slate-800"
                      aria-label="Cancelar"
                    >
                      <X className="h-4 w-4" />
                    </button>
                  )}
                </div>
              )}
              {subjectError && (
                <p role="alert" className="mt-2 text-xs font-semibold text-rose-600 dark:text-rose-400">
                  {subjectError}
                </p>
              )}
            </fieldset>

            <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
              Título
              <input
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                maxLength={80}
                required
                className={inputClasses}
                placeholder="Ej. Ejercicios página 45"
              />
            </label>

            <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
              Descripción (opcional)
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                rows={3}
                className={`${inputClasses} resize-none`}
                placeholder="Detalles de la tarea..."
              />
            </label>

            <div className="mt-4">
              <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">Fecha de entrega</span>
              <div className="mt-1.5">
                <DatePicker value={dueDate} onChange={setDueDate} />
              </div>
            </div>

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
              {submitting
                ? editing
                  ? 'Guardando…'
                  : 'Enviando…'
                : editing
                  ? 'Guardar cambios'
                  : canPublishOfficial
                    ? 'Publicar tarea'
                    : 'Enviar propuesta'}
            </button>
          </>
        )}
      </form>
    </div>
  );
}

import { useRef, useState, type FormEvent, type MouseEvent } from 'react';
import { X } from 'lucide-react';
import { useStudentRepository } from '@/hooks/useStudentRepository';
import { getApiErrorMessage } from '@/lib/apiError';
import { MAX_STUDENT_NAME_PART_LENGTH } from '@/lib/validation';
import type { Child } from '@/modules/students/domain/Child';
import { TextField } from './TextField';

type FieldErrors = { firstName?: string; firstSurname?: string };

export function ChildNameModal({
  child,
  onSaved,
  onClose,
}: {
  child: Child;
  onSaved: () => Promise<void>;
  onClose: () => void;
}) {
  const studentRepository = useStudentRepository();
  const [firstName, setFirstName] = useState(child.firstName);
  const [firstSurname, setFirstSurname] = useState(child.firstSurname);
  const [secondSurname, setSecondSurname] = useState(child.secondSurname ?? '');
  const [errors, setErrors] = useState<FieldErrors>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const mouseDownOnBackdrop = useRef(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (saving) return;
    const found: FieldErrors = {};
    if (!firstName.trim()) found.firstName = 'Indica el nombre.';
    if (!firstSurname.trim()) found.firstSurname = 'Indica el primer apellido.';
    setErrors(found);
    setSubmitError(null);
    if (Object.keys(found).length > 0) return;

    setSaving(true);
    try {
      await studentRepository.changeName(child.id, {
        firstName: firstName.trim(),
        firstSurname: firstSurname.trim(),
        secondSurname: secondSurname.trim() || undefined,
      });
    } catch (err) {
      setSubmitError(getApiErrorMessage(err));
      setSaving(false);
      return;
    }
    await onSaved();
    onClose();
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-end justify-center bg-slate-900/40 p-0 backdrop-blur-sm sm:items-center sm:p-4"
      onMouseDown={(e: MouseEvent) => {
        mouseDownOnBackdrop.current = e.target === e.currentTarget;
      }}
      onClick={(e: MouseEvent) => {
        if (mouseDownOnBackdrop.current && e.target === e.currentTarget && !saving) onClose();
      }}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="child-name-modal-title"
        className="max-h-[85vh] w-full max-w-lg overflow-y-auto rounded-t-3xl border border-slate-200 bg-white p-6 shadow-xl dark:border-slate-700 dark:bg-slate-900 sm:rounded-3xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-start justify-between gap-3">
          <h2 id="child-name-modal-title" className="font-display text-lg font-bold text-slate-900 dark:text-white">
            Editar el nombre de {child.firstName}
          </h2>
          <button
            type="button"
            onClick={onClose}
            disabled={saving}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 disabled:opacity-50 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>
        <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">
          En el resto de pantallas solo se verá el nombre y la inicial del primer apellido.
        </p>

        <form onSubmit={handleSubmit} noValidate className="mt-4 space-y-4">
          <TextField
            id="child-first-name"
            label="Nombre"
            value={firstName}
            onChange={setFirstName}
            maxLength={MAX_STUDENT_NAME_PART_LENGTH}
            required
            error={errors.firstName}
          />
          <TextField
            id="child-first-surname"
            label="Primer apellido"
            value={firstSurname}
            onChange={setFirstSurname}
            maxLength={MAX_STUDENT_NAME_PART_LENGTH}
            required
            error={errors.firstSurname}
          />
          <TextField
            id="child-second-surname"
            label="Segundo apellido (opcional)"
            value={secondSurname}
            onChange={setSecondSurname}
            maxLength={MAX_STUDENT_NAME_PART_LENGTH}
          />
          {submitError && (
            <p
              role="alert"
              className="rounded-xl border border-rose-200 bg-rose-50 px-3.5 py-2.5 text-sm text-rose-700 dark:border-rose-700/40 dark:bg-rose-900/20 dark:text-rose-300"
            >
              {submitError}
            </p>
          )}
          <div className="flex flex-wrap justify-end gap-2">
            <button
              type="button"
              onClick={onClose}
              disabled={saving}
              className="rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-50 disabled:opacity-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={saving}
              className="rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {saving ? 'Guardando…' : 'Guardar cambios'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

import { useId, useState, type FormEvent } from 'react';
import { AlertTriangle } from 'lucide-react';
import { getApiErrorMessage } from '@/lib/apiError';

const CHILD_DATA_WARNING = 'No escribas nombres de niños ni teléfonos.';

export function ClaimTextForm({
  title,
  hint,
  label,
  placeholder,
  maxLength,
  required,
  submitLabel,
  submittingLabel,
  onSubmit,
  onCancel,
}: {
  title: string;
  hint: string;
  label: string;
  placeholder?: string;
  maxLength: number;
  required: boolean;
  submitLabel: string;
  submittingLabel: string;
  onSubmit: (text: string) => Promise<void>;
  onCancel: () => void;
}) {
  const [text, setText] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const fieldId = useId();

  const trimmed = text.trim();
  const canSubmit = (!required || trimmed.length > 0) && !submitting;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!canSubmit) return;
    setSubmitting(true);
    setError(null);
    try {
      await onSubmit(trimmed);
    } catch (err) {
      setError(getApiErrorMessage(err));
      setSubmitting(false);
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="mt-4 rounded-2xl border border-slate-200 bg-slate-50/60 p-4 dark:border-slate-700 dark:bg-slate-800/40"
    >
      <p className="text-sm font-semibold text-slate-900 dark:text-white">{title}</p>
      <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">{hint}</p>
      <p className="mt-2 flex items-start gap-1.5 text-xs font-semibold text-sun-700 dark:text-sun-300">
        <AlertTriangle className="mt-0.5 h-3.5 w-3.5 shrink-0" />
        {CHILD_DATA_WARNING}
      </p>

      <label htmlFor={fieldId} className="mt-3 block text-sm font-semibold text-slate-700 dark:text-slate-300">
        {label}
      </label>
      <textarea
        id={fieldId}
        value={text}
        onChange={(e) => setText(e.target.value)}
        rows={3}
        maxLength={maxLength}
        required={required}
        placeholder={placeholder}
        className="mt-1.5 w-full resize-none rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white"
      />
      <p className="mt-1 text-right text-xs text-slate-500 dark:text-slate-400" aria-live="polite">
        {text.length}/{maxLength}
      </p>

      {error && (
        <p role="alert" className="mt-2 text-sm font-semibold text-rose-600 dark:text-rose-400">
          {error}
        </p>
      )}

      <div className="mt-3 flex flex-wrap gap-2">
        <button
          type="submit"
          disabled={!canSubmit}
          className="rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-3 py-2 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 transition-opacity disabled:cursor-not-allowed disabled:opacity-50"
        >
          {submitting ? submittingLabel : submitLabel}
        </button>
        <button
          type="button"
          onClick={onCancel}
          disabled={submitting}
          className="rounded-xl border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-50 disabled:opacity-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800"
        >
          Cancelar
        </button>
      </div>
    </form>
  );
}

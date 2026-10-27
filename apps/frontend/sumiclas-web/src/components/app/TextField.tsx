import type { ReactNode } from 'react';

const READ_ONLY_CLASS = 'border-slate-200 bg-slate-100 dark:border-slate-700 dark:bg-slate-800';
const INVALID_CLASS =
  'border-rose-300 bg-slate-50 focus-within:border-rose-400 focus-within:ring-rose-100 dark:border-rose-700/60 dark:bg-slate-800/60 dark:focus-within:ring-rose-900/40';
const DEFAULT_CLASS =
  'border-slate-200 bg-slate-50 focus-within:border-brand-400 focus-within:bg-white focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40';

export function TextField({
  id,
  label,
  value,
  onChange,
  type = 'text',
  autoComplete,
  maxLength,
  readOnly = false,
  required = false,
  error,
  hint,
  adornment,
}: {
  id: string;
  label: string;
  value: string;
  onChange?: (value: string) => void;
  type?: 'text' | 'email' | 'password';
  autoComplete?: string;
  maxLength?: number;
  readOnly?: boolean;
  required?: boolean;
  error?: string;
  hint?: string;
  adornment?: ReactNode;
}) {
  const describedBy = error ? `${id}-error` : hint ? `${id}-hint` : undefined;
  const stateClass = readOnly ? READ_ONLY_CLASS : error ? INVALID_CLASS : DEFAULT_CLASS;
  return (
    <div>
      <label htmlFor={id} className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300">
        {label}
      </label>
      <div className={`flex items-center gap-2 rounded-xl border px-3 py-2.5 transition-colors focus-within:ring-2 ${stateClass}`}>
        <input
          id={id}
          type={type}
          autoComplete={autoComplete}
          value={value}
          maxLength={maxLength}
          readOnly={readOnly}
          required={required}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
          onChange={(e) => onChange?.(e.target.value)}
          className={`w-full bg-transparent text-sm focus:outline-none ${
            readOnly ? 'text-slate-500 dark:text-slate-400' : 'text-slate-700 dark:text-slate-200'
          }`}
        />
        {adornment}
      </div>
      {error ? (
        <p id={`${id}-error`} className="mt-1 text-xs text-rose-600 dark:text-rose-400">
          {error}
        </p>
      ) : (
        hint && (
          <p id={`${id}-hint`} className="mt-1 text-xs text-slate-500 dark:text-slate-400">
            {hint}
          </p>
        )
      )}
    </div>
  );
}

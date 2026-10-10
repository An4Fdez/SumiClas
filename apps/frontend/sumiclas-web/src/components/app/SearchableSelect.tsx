import { useState } from 'react';
import { ChevronDown } from 'lucide-react';
import { normalizeText } from '@/lib/text';

const MAX_VISIBLE_OPTIONS = 100;

/**
 * A select with a text filter for long lists (provinces, localities). The panel is not portaled, so use it outside
 * modals. Options are filtered on the client, ignoring case and accents.
 */
export function SearchableSelect({
  value,
  options,
  placeholder,
  disabled,
  loading,
  ariaLabel,
  onChange,
}: {
  value: string;
  options: { value: string; label: string }[];
  placeholder: string;
  disabled?: boolean;
  loading?: boolean;
  ariaLabel: string;
  onChange: (value: string) => void;
}) {
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState('');

  const selectedLabel = options.find((o) => o.value === value)?.label ?? '';
  const normalizedQuery = normalizeText(query);
  const filtered = normalizedQuery ? options.filter((o) => normalizeText(o.label).includes(normalizedQuery)) : options;
  const visible = filtered.slice(0, MAX_VISIBLE_OPTIONS);

  const choose = (next: string) => {
    onChange(next);
    setOpen(false);
    setQuery('');
  };

  return (
    <div className="relative">
      <div
        className={`flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2.5 focus-within:border-brand-400 focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-900 ${
          disabled ? 'opacity-60' : ''
        }`}
      >
        <input
          type="text"
          role="combobox"
          aria-label={ariaLabel}
          aria-expanded={open}
          disabled={disabled}
          value={open ? query : selectedLabel}
          placeholder={loading ? 'Cargando…' : placeholder}
          onFocus={() => {
            setQuery('');
            setOpen(true);
          }}
          onBlur={() => {
            setOpen(false);
            setQuery('');
          }}
          onChange={(e) => setQuery(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Escape') setOpen(false);
            if (e.key === 'Enter') {
              e.preventDefault();
              if (visible[0]) choose(visible[0].value);
            }
          }}
          className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
        />
        <ChevronDown className={`h-3.5 w-3.5 shrink-0 text-slate-400 transition-transform ${open ? 'rotate-180' : ''}`} />
      </div>

      {open && (
        <div
          role="listbox"
          className="absolute inset-x-0 top-full z-30 mt-2 max-h-60 overflow-y-auto rounded-2xl border border-slate-200 bg-white p-1.5 shadow-xl dark:border-slate-700 dark:bg-slate-900"
        >
          {visible.length === 0 ? (
            <p className="px-3 py-3 text-sm text-slate-500 dark:text-slate-400">Sin resultados.</p>
          ) : (
            visible.map((o) => (
              <button
                key={o.value}
                type="button"
                role="option"
                aria-selected={o.value === value}
                onMouseDown={(e) => e.preventDefault()}
                onClick={() => choose(o.value)}
                className={`flex w-full items-center rounded-xl px-3 py-2 text-left text-sm font-semibold transition-colors hover:bg-slate-50 dark:hover:bg-slate-800 ${
                  o.value === value
                    ? 'bg-brand-50 text-brand-700 dark:bg-brand-900/20 dark:text-brand-300'
                    : 'text-slate-700 dark:text-slate-300'
                }`}
              >
                {o.label}
              </button>
            ))
          )}
          {filtered.length > visible.length && (
            <p className="px-3 py-2 text-xs text-slate-400 dark:text-slate-500">
              Mostrando {visible.length} de {filtered.length}. Escribe para acotar.
            </p>
          )}
        </div>
      )}
    </div>
  );
}

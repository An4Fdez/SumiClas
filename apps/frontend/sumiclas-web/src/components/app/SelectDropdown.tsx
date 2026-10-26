import { useRef, useState } from 'react';
import { ChevronDown } from 'lucide-react';
import { FloatingPanel } from './FloatingPanel';

export function SelectDropdown({
  value,
  options,
  placeholder,
  onChange,
}: {
  value: string;
  options: { value: string; label: string }[];
  placeholder?: string;
  onChange: (value: string) => void;
}) {
  const [open, setOpen] = useState(false);
  const triggerRef = useRef<HTMLButtonElement>(null);

  const label = options.find((o) => o.value === value)?.label ?? placeholder ?? '';

  return (
    <>
      <button
        ref={triggerRef}
        type="button"
        onClick={() => setOpen((o) => !o)}
        className={`flex w-full items-center justify-between gap-1.5 rounded-xl border border-slate-200 bg-white px-3 py-2 text-left text-sm text-slate-900 transition-colors focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white ${
          !value ? 'text-slate-400 dark:text-slate-500' : ''
        }`}
      >
        <span className="truncate">{label}</span>
        <ChevronDown className={`h-3.5 w-3.5 shrink-0 text-slate-400 transition-transform ${open ? 'rotate-180' : ''}`} />
      </button>

      {open && (
        <FloatingPanel
          triggerRef={triggerRef}
          onClose={() => setOpen(false)}
          className="z-[60] max-h-60 overflow-y-auto rounded-2xl border border-slate-200 bg-white p-1.5 shadow-xl dark:border-slate-700 dark:bg-slate-900"
        >
          {options.map((op) => (
            <button
              key={op.value}
              type="button"
              onClick={() => {
                onChange(op.value);
                setOpen(false);
              }}
              className={`flex w-full items-center rounded-xl px-3 py-2 text-left text-sm font-semibold transition-colors hover:bg-slate-50 dark:hover:bg-slate-800 ${
                value === op.value ? 'bg-brand-50 text-brand-700 dark:bg-brand-900/20 dark:text-brand-300' : 'text-slate-700 dark:text-slate-300'
              }`}
            >
              {op.label}
            </button>
          ))}
        </FloatingPanel>
      )}
    </>
  );
}

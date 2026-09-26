import { useState, type FocusEvent } from 'react';
import { Check, ChevronDown } from 'lucide-react';

export function ParticipantsDropdown({
  availableParticipants,
  selected,
  onChange,
}: {
  availableParticipants: { id: string; name: string }[];
  selected: Set<string>;
  onChange: (next: Set<string>) => void;
}) {
  const [open, setOpen] = useState(false);

  const allSelected = selected.size === availableParticipants.length;
  const label = allSelected
    ? `Todos participan (${availableParticipants.length})`
    : `${selected.size} de ${availableParticipants.length} participan`;

  const toggle = (id: string) => {
    const next = new Set(selected);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    onChange(next);
  };

  const selectAll = () => onChange(new Set(availableParticipants.map((p) => p.id)));

  const handleBlur = (e: FocusEvent<HTMLDivElement>) => {
    if (!e.currentTarget.contains(e.relatedTarget as Node)) setOpen(false);
  };

  return (
    <div className="relative" onBlur={handleBlur}>
      <button
        type="button"
        onClick={() => setOpen((o) => !o)}
        className="inline-flex w-full items-center justify-between gap-1.5 rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-700 transition-colors hover:bg-slate-50 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700"
      >
        {label}
        <ChevronDown className={`h-3.5 w-3.5 shrink-0 transition-transform ${open ? 'rotate-180' : ''}`} />
      </button>

      {open && (
        <div className="absolute left-0 top-full z-30 mt-2 w-full min-w-[16rem] rounded-2xl border border-slate-200 bg-white p-1.5 shadow-xl dark:border-slate-700 dark:bg-slate-900">
          <button
            type="button"
            onClick={selectAll}
            className={`flex w-full items-center rounded-xl px-3 py-2 text-left text-sm font-semibold transition-colors hover:bg-slate-50 dark:hover:bg-slate-800 ${
              allSelected ? 'text-brand-600 dark:text-brand-400' : 'text-slate-700 dark:text-slate-300'
            }`}
          >
            Seleccionar todos
          </button>

          <div className="my-1 h-px bg-slate-100 dark:bg-slate-800" />

          <div className="max-h-56 overflow-y-auto">
            {availableParticipants.map((p) => {
              const active = selected.has(p.id);
              return (
                <button
                  key={p.id}
                  type="button"
                  onClick={() => toggle(p.id)}
                  className="flex w-full items-center gap-2.5 rounded-xl px-3 py-2 text-left text-sm font-semibold text-slate-700 transition-colors hover:bg-slate-50 dark:text-slate-300 dark:hover:bg-slate-800"
                >
                  <span
                    className={`grid h-4 w-4 shrink-0 place-items-center rounded border ${
                      active
                        ? 'border-brand-500 bg-brand-500 text-white'
                        : 'border-slate-300 dark:border-slate-600'
                    }`}
                  >
                    {active && <Check className="h-3 w-3" />}
                  </span>
                  {p.name}
                </button>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}

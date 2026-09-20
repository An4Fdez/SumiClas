import { useState, type FocusEvent } from 'react';
import { Check, ChevronDown } from 'lucide-react';
import type { AsignaturaInfo } from '@/data/deberesMock';

export function FiltroAsignaturaDropdown({
  asignaturas,
  seleccionadas,
  onCambiar,
}: {
  asignaturas: AsignaturaInfo[];
  seleccionadas: Set<string>;
  onCambiar: (siguiente: Set<string>) => void;
}) {
  const [open, setOpen] = useState(false);

  const label =
    seleccionadas.size === 0
      ? 'Todas las asignaturas'
      : seleccionadas.size === 1
        ? (asignaturas.find((a) => a.id === [...seleccionadas][0])?.label ?? '1 asignatura')
        : `${seleccionadas.size} asignaturas`;

  const toggle = (id: string) => {
    const next = new Set(seleccionadas);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    onCambiar(next);
  };

  const handleBlur = (e: FocusEvent<HTMLDivElement>) => {
    if (!e.currentTarget.contains(e.relatedTarget as Node)) setOpen(false);
  };

  return (
    <div className="relative" onBlur={handleBlur}>
      <button
        type="button"
        onClick={() => setOpen((o) => !o)}
        className={`inline-flex items-center gap-1.5 rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
          seleccionadas.size > 0
            ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
            : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
        }`}
      >
        {label}
        <ChevronDown className={`h-3.5 w-3.5 transition-transform ${open ? 'rotate-180' : ''}`} />
      </button>

      {open && (
        <div className="absolute left-0 top-full z-30 mt-2 w-56 rounded-2xl border border-slate-200 bg-white p-1.5 shadow-xl dark:border-slate-700 dark:bg-slate-900">
          <button
            type="button"
            onClick={() => onCambiar(new Set())}
            className={`flex w-full items-center rounded-xl px-3 py-2 text-left text-sm font-semibold transition-colors hover:bg-slate-50 dark:hover:bg-slate-800 ${
              seleccionadas.size === 0 ? 'text-brand-600 dark:text-brand-400' : 'text-slate-700 dark:text-slate-300'
            }`}
          >
            Todas las asignaturas
          </button>

          <div className="my-1 h-px bg-slate-100 dark:bg-slate-800" />

          {asignaturas.map((asignatura) => {
            const activa = seleccionadas.has(asignatura.id);
            return (
              <button
                key={asignatura.id}
                type="button"
                onClick={() => toggle(asignatura.id)}
                className="flex w-full items-center gap-2.5 rounded-xl px-3 py-2 text-left text-sm font-semibold text-slate-700 transition-colors hover:bg-slate-50 dark:text-slate-300 dark:hover:bg-slate-800"
              >
                <span
                  className={`grid h-4 w-4 shrink-0 place-items-center rounded border ${
                    activa
                      ? 'border-brand-500 bg-brand-500 text-white'
                      : 'border-slate-300 dark:border-slate-600'
                  }`}
                >
                  {activa && <Check className="h-3 w-3" />}
                </span>
                {asignatura.label}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}

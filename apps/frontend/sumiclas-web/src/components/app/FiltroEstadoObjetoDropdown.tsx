import { useState, type FocusEvent } from 'react';
import { ChevronDown } from 'lucide-react';

export type FiltroEstadoObjeto = 'activos' | 'buscado' | 'encontrado' | 'recuperado';

const opciones: { value: FiltroEstadoObjeto; label: string }[] = [
  { value: 'activos', label: 'Activos' },
  { value: 'buscado', label: 'Buscados' },
  { value: 'encontrado', label: 'Encontrados' },
  { value: 'recuperado', label: 'Recuperados' },
];

export function FiltroEstadoObjetoDropdown({
  valor,
  onCambiar,
}: {
  valor: FiltroEstadoObjeto;
  onCambiar: (siguiente: FiltroEstadoObjeto) => void;
}) {
  const [open, setOpen] = useState(false);
  const label = opciones.find((o) => o.value === valor)?.label ?? 'Estado';

  const handleBlur = (e: FocusEvent<HTMLDivElement>) => {
    if (!e.currentTarget.contains(e.relatedTarget as Node)) setOpen(false);
  };

  return (
    <div className="relative" onBlur={handleBlur}>
      <button
        type="button"
        onClick={() => setOpen((o) => !o)}
        className={`inline-flex items-center gap-1.5 rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
          valor !== 'activos'
            ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
            : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
        }`}
      >
        {label}
        <ChevronDown className={`h-3.5 w-3.5 transition-transform ${open ? 'rotate-180' : ''}`} />
      </button>

      {open && (
        <div className="absolute left-0 top-full z-30 mt-2 w-44 rounded-2xl border border-slate-200 bg-white p-1.5 shadow-xl dark:border-slate-700 dark:bg-slate-900">
          {opciones.map((op) => (
            <button
              key={op.value}
              type="button"
              onClick={() => {
                onCambiar(op.value);
                setOpen(false);
              }}
              className={`flex w-full items-center rounded-xl px-3 py-2 text-left text-sm font-semibold transition-colors hover:bg-slate-50 dark:hover:bg-slate-800 ${
                valor === op.value ? 'text-brand-600 dark:text-brand-400' : 'text-slate-700 dark:text-slate-300'
              }`}
            >
              {op.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

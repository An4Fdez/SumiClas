import { useState, type FocusEvent } from 'react';
import { Check, ChevronDown } from 'lucide-react';

export function ParticipantesDropdown({
  participantesDisponibles,
  seleccionados,
  onCambiar,
}: {
  participantesDisponibles: { id: string; nombre: string }[];
  seleccionados: Set<string>;
  onCambiar: (siguiente: Set<string>) => void;
}) {
  const [open, setOpen] = useState(false);

  const todosSeleccionados = seleccionados.size === participantesDisponibles.length;
  const label = todosSeleccionados
    ? `Todos participan (${participantesDisponibles.length})`
    : `${seleccionados.size} de ${participantesDisponibles.length} participan`;

  const toggle = (id: string) => {
    const next = new Set(seleccionados);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    onCambiar(next);
  };

  const seleccionarTodos = () => onCambiar(new Set(participantesDisponibles.map((p) => p.id)));

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
            onClick={seleccionarTodos}
            className={`flex w-full items-center rounded-xl px-3 py-2 text-left text-sm font-semibold transition-colors hover:bg-slate-50 dark:hover:bg-slate-800 ${
              todosSeleccionados ? 'text-brand-600 dark:text-brand-400' : 'text-slate-700 dark:text-slate-300'
            }`}
          >
            Seleccionar todos
          </button>

          <div className="my-1 h-px bg-slate-100 dark:bg-slate-800" />

          <div className="max-h-56 overflow-y-auto">
            {participantesDisponibles.map((p) => {
              const activo = seleccionados.has(p.id);
              return (
                <button
                  key={p.id}
                  type="button"
                  onClick={() => toggle(p.id)}
                  className="flex w-full items-center gap-2.5 rounded-xl px-3 py-2 text-left text-sm font-semibold text-slate-700 transition-colors hover:bg-slate-50 dark:text-slate-300 dark:hover:bg-slate-800"
                >
                  <span
                    className={`grid h-4 w-4 shrink-0 place-items-center rounded border ${
                      activo
                        ? 'border-brand-500 bg-brand-500 text-white'
                        : 'border-slate-300 dark:border-slate-600'
                    }`}
                  >
                    {activo && <Check className="h-3 w-3" />}
                  </span>
                  {p.nombre}
                </button>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}

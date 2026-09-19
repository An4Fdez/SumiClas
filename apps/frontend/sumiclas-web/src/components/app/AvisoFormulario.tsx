import { useRef, useState, type FormEvent, type MouseEvent } from 'react';
import { X } from 'lucide-react';
import type { AvisoPrioridad, EventoRelacionado } from '@/data/avisosMock';

const prioridadOpciones: { value: AvisoPrioridad; label: string }[] = [
  { value: 'baja', label: 'Baja' },
  { value: 'normal', label: 'Normal' },
  { value: 'alta', label: 'Alta (urgente)' },
];

export function AvisoFormulario({
  claseNombre,
  eventos,
  onSubmit,
  onClose,
}: {
  claseNombre: string;
  eventos: EventoRelacionado[];
  onSubmit: (data: { titulo: string; cuerpo: string; prioridad: AvisoPrioridad; eventoId: string | null }) => void;
  onClose: () => void;
}) {
  const [titulo, setTitulo] = useState('');
  const [cuerpo, setCuerpo] = useState('');
  const [prioridad, setPrioridad] = useState<AvisoPrioridad>('normal');
  const [eventoId, setEventoId] = useState('');
  const mouseDownEnFondo = useRef(false);

  const puedeEnviar = titulo.trim().length > 0 && cuerpo.trim().length > 0;

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!puedeEnviar) return;
    onSubmit({ titulo: titulo.trim(), cuerpo: cuerpo.trim(), prioridad, eventoId: eventoId || null });
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-end justify-center bg-slate-900/40 p-0 backdrop-blur-sm sm:items-center sm:p-4"
      onMouseDown={(e: MouseEvent) => {
        mouseDownEnFondo.current = e.target === e.currentTarget;
      }}
      onClick={(e: MouseEvent) => {
        if (mouseDownEnFondo.current && e.target === e.currentTarget) onClose();
      }}
    >
      <form
        onSubmit={handleSubmit}
        onClick={(e) => e.stopPropagation()}
        className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-t-3xl border border-slate-200 bg-white p-6 shadow-xl dark:border-slate-700 dark:bg-slate-900 sm:rounded-3xl"
      >
        <div className="flex items-center justify-between">
          <h2 className="font-display text-xl font-extrabold text-slate-900 dark:text-white">Nuevo aviso</h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">Se publicará para {claseNombre}.</p>

        <label className="mt-5 block text-sm font-semibold text-slate-700 dark:text-slate-300">
          Título
          <input
            value={titulo}
            onChange={(e) => setTitulo(e.target.value)}
            maxLength={80}
            required
            className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white"
            placeholder="Ej. Examen de matemáticas el viernes"
          />
        </label>

        <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
          Mensaje
          <textarea
            value={cuerpo}
            onChange={(e) => setCuerpo(e.target.value)}
            required
            rows={4}
            className="mt-1.5 w-full resize-none rounded-xl border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white"
            placeholder="Escribe el mensaje para las familias..."
          />
        </label>

        <fieldset className="mt-4">
          <legend className="text-sm font-semibold text-slate-700 dark:text-slate-300">Prioridad</legend>
          <div className="mt-1.5 flex gap-2">
            {prioridadOpciones.map((op) => (
              <button
                key={op.value}
                type="button"
                onClick={() => setPrioridad(op.value)}
                className={`flex-1 rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                  prioridad === op.value
                    ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/20 dark:text-brand-300'
                    : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
                }`}
              >
                {op.label}
              </button>
            ))}
          </div>
          {prioridad === 'alta' && (
            <p className="mt-1.5 text-xs text-rose-600 dark:text-rose-400">
              Se enviará una notificación push inmediata a todas las familias.
            </p>
          )}
        </fieldset>

        {eventos.length > 0 && (
          <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
            Evento relacionado (opcional)
            <select
              value={eventoId}
              onChange={(e) => setEventoId(e.target.value)}
              className="mt-1.5 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white"
            >
              <option value="">Ninguno</option>
              {eventos.map((ev) => (
                <option key={ev.id} value={ev.id}>
                  {ev.titulo}
                </option>
              ))}
            </select>
          </label>
        )}

        <button
          type="submit"
          disabled={!puedeEnviar}
          className="mt-6 w-full rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 transition-opacity disabled:cursor-not-allowed disabled:opacity-50"
        >
          Publicar aviso
        </button>
      </form>
    </div>
  );
}

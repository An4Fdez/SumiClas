import { useRef, useState, type FormEvent, type KeyboardEvent, type MouseEvent } from 'react';
import { Plus, X } from 'lucide-react';
import type { AsignaturaInfo } from '@/data/deberesMock';
import { DatePicker } from './DatePicker';

export type NuevaTarea = {
  asignaturaId: string;
  titulo: string;
  descripcion: string;
  fecha: string;
};

const inputClasses =
  'mt-1.5 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white';

export function TareaFormulario({
  puedeCompleto,
  claseNombre,
  asignaturas,
  onCrearAsignatura,
  onSubmit,
  onClose,
}: {
  puedeCompleto: boolean;
  claseNombre: string;
  asignaturas: AsignaturaInfo[];
  onCrearAsignatura: (nombre: string) => AsignaturaInfo;
  onSubmit: (data: NuevaTarea) => void;
  onClose: () => void;
}) {
  const [asignaturaId, setAsignaturaId] = useState(asignaturas[0]?.id ?? '');
  const [creandoAsignatura, setCreandoAsignatura] = useState(false);
  const [nombreNueva, setNombreNueva] = useState('');
  const [titulo, setTitulo] = useState('');
  const [descripcion, setDescripcion] = useState('');
  const [fecha, setFecha] = useState('');
  const mouseDownEnFondo = useRef(false);

  const puedeEnviar = asignaturaId.length > 0 && titulo.trim().length > 0 && fecha.length > 0;

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!puedeEnviar) return;
    onSubmit({ asignaturaId, titulo: titulo.trim(), descripcion: descripcion.trim(), fecha });
  };

  const confirmarNuevaAsignatura = () => {
    const nombre = nombreNueva.trim();
    if (!nombre) return;
    const nueva = onCrearAsignatura(nombre);
    setAsignaturaId(nueva.id);
    setNombreNueva('');
    setCreandoAsignatura(false);
  };

  const handleKeyDownNueva = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      confirmarNuevaAsignatura();
    }
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
          <h2 className="font-display text-xl font-extrabold text-slate-900 dark:text-white">
            {puedeCompleto ? 'Nueva tarea' : 'Proponer tarea'}
          </h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">
          {puedeCompleto
            ? `Se publicará como tarea oficial para ${claseNombre}.`
            : `Se mostrará como propuesta de familia para ${claseNombre}.`}
        </p>

        <fieldset className="mt-5">
          <legend className="text-sm font-semibold text-slate-700 dark:text-slate-300">Asignatura</legend>
          <div className="mt-1.5 flex flex-wrap gap-2">
            {asignaturas.map((a) => (
              <button
                key={a.id}
                type="button"
                onClick={() => setAsignaturaId(a.id)}
                className={`rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                  asignaturaId === a.id
                    ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/20 dark:text-brand-300'
                    : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
                }`}
              >
                {a.label}
              </button>
            ))}
            {!creandoAsignatura && (
              <button
                type="button"
                onClick={() => setCreandoAsignatura(true)}
                className="inline-flex items-center gap-1 rounded-xl border border-dashed border-slate-300 px-3 py-2 text-sm font-semibold text-slate-500 transition-colors hover:bg-slate-50 dark:border-slate-600 dark:text-slate-400 dark:hover:bg-slate-800"
              >
                <Plus className="h-3.5 w-3.5" />
                Nueva
              </button>
            )}
          </div>

          {creandoAsignatura && (
            <div className="mt-2 flex items-center gap-2">
              <input
                value={nombreNueva}
                onChange={(e) => setNombreNueva(e.target.value)}
                onKeyDown={handleKeyDownNueva}
                autoFocus
                maxLength={30}
                placeholder="Nombre de la asignatura"
                className="flex-1 rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white"
              />
              <button
                type="button"
                onClick={confirmarNuevaAsignatura}
                disabled={!nombreNueva.trim()}
                className="rounded-xl bg-brand-600 px-3 py-2 text-sm font-semibold text-white transition-opacity disabled:cursor-not-allowed disabled:opacity-50"
              >
                Añadir
              </button>
              <button
                type="button"
                onClick={() => {
                  setCreandoAsignatura(false);
                  setNombreNueva('');
                }}
                className="rounded-full p-2 text-slate-400 transition-colors hover:bg-slate-100 dark:hover:bg-slate-800"
                aria-label="Cancelar"
              >
                <X className="h-4 w-4" />
              </button>
            </div>
          )}
        </fieldset>

        <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
          Título
          <input
            value={titulo}
            onChange={(e) => setTitulo(e.target.value)}
            maxLength={80}
            required
            className={inputClasses}
            placeholder="Ej. Ejercicios página 45"
          />
        </label>

        <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
          Descripción (opcional)
          <textarea
            value={descripcion}
            onChange={(e) => setDescripcion(e.target.value)}
            rows={3}
            className={`${inputClasses} resize-none`}
            placeholder="Detalles de la tarea..."
          />
        </label>

        <div className="mt-4">
          <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">Fecha de entrega</span>
          <div className="mt-1.5">
            <DatePicker value={fecha} onChange={setFecha} />
          </div>
        </div>

        <button
          type="submit"
          disabled={!puedeEnviar}
          className="mt-6 w-full rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 transition-opacity disabled:cursor-not-allowed disabled:opacity-50"
        >
          {puedeCompleto ? 'Publicar tarea' : 'Enviar propuesta'}
        </button>
      </form>
    </div>
  );
}

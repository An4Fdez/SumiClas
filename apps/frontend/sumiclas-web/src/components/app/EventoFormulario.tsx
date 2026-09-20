import { useRef, useState, type FormEvent, type MouseEvent } from 'react';
import { X } from 'lucide-react';
import type { HijoContexto } from '@/data/appMock';
import { nombresMeses } from '@/lib/date';
import { tipoEventoMeta, type EventoTipo } from '@/data/calendarioMock';

const todosLosTipos: EventoTipo[] = ['excursion', 'reunion', 'actividad', 'otros', 'cumpleanos'];

export type NuevoEventoOficial = {
  tipo: 'excursion' | 'reunion' | 'actividad';
  titulo: string;
  fecha: string;
  hora: string;
  ubicacion: string;
  coste: string;
};

export type NuevoEventoOtros = {
  tipo: 'otros';
  alcance: 'clase' | 'colegio';
  titulo: string;
  fecha: string;
  hora: string;
  ubicacion: string;
  coste: string;
};

export type NuevoCumpleanos = {
  tipo: 'cumpleanos';
  hijoId: string;
  dia: number;
  mes: number;
};

export type NuevoEvento = NuevoEventoOficial | NuevoEventoOtros | NuevoCumpleanos;

const inputClasses =
  'mt-1.5 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white';

export function EventoFormulario({
  puedeCompleto,
  claseNombre,
  hijos,
  hijoInicialId,
  onSubmit,
  onClose,
}: {
  puedeCompleto: boolean;
  claseNombre: string;
  hijos: HijoContexto[];
  hijoInicialId: string;
  onSubmit: (data: NuevoEvento) => void;
  onClose: () => void;
}) {
  const [tipo, setTipo] = useState<EventoTipo>(puedeCompleto ? 'excursion' : 'cumpleanos');
  const [titulo, setTitulo] = useState('');
  const [fecha, setFecha] = useState('');
  const [hora, setHora] = useState('');
  const [ubicacion, setUbicacion] = useState('');
  const [coste, setCoste] = useState('');
  const [alcance, setAlcance] = useState<'clase' | 'colegio'>('clase');
  const [hijoId, setHijoId] = useState(hijoInicialId);
  const hoy = new Date();
  const [diaCumple, setDiaCumple] = useState(hoy.getDate());
  const [mesCumple, setMesCumple] = useState(hoy.getMonth() + 1);
  const mouseDownEnFondo = useRef(false);

  const esCumpleanos = tipo === 'cumpleanos';
  const puedeEnviar = esCumpleanos ? hijoId.length > 0 : titulo.trim().length > 0 && fecha.length > 0;
  const diasDelMes = new Date(hoy.getFullYear(), mesCumple, 0).getDate();

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!puedeEnviar) return;
    if (esCumpleanos) {
      onSubmit({ tipo: 'cumpleanos', hijoId, dia: diaCumple, mes: mesCumple });
    } else if (tipo === 'otros') {
      onSubmit({
        tipo: 'otros',
        alcance,
        titulo: titulo.trim(),
        fecha,
        hora,
        ubicacion: ubicacion.trim(),
        coste: coste.trim(),
      });
    } else {
      onSubmit({
        tipo: tipo as 'excursion' | 'reunion' | 'actividad',
        titulo: titulo.trim(),
        fecha,
        hora,
        ubicacion: ubicacion.trim(),
        coste: coste.trim(),
      });
    }
  };

  const handleCambiarMes = (nuevoMes: number) => {
    setMesCumple(nuevoMes);
    const max = new Date(hoy.getFullYear(), nuevoMes, 0).getDate();
    if (diaCumple > max) setDiaCumple(max);
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
            {esCumpleanos ? 'Añadir cumpleaños' : 'Nuevo evento'}
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
          {esCumpleanos
            ? `Se mostrará cada año en el calendario de ${claseNombre}, sin guardar el año de nacimiento.`
            : tipo === 'otros' && alcance === 'colegio'
              ? 'Se publicará para todo el colegio.'
              : `Se publicará para ${claseNombre}.`}
        </p>

        {puedeCompleto && (
          <fieldset className="mt-5">
            <legend className="text-sm font-semibold text-slate-700 dark:text-slate-300">Tipo</legend>
            <div className="mt-1.5 flex flex-wrap gap-2">
              {todosLosTipos.map((t) => (
                <button
                  key={t}
                  type="button"
                  onClick={() => setTipo(t)}
                  className={`rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                    tipo === t
                      ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/20 dark:text-brand-300'
                      : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
                  }`}
                >
                  {tipoEventoMeta[t].label}
                </button>
              ))}
            </div>
          </fieldset>
        )}

        {esCumpleanos ? (
          <>
            {hijos.length > 1 && (
              <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
                Hijo/a
                <select value={hijoId} onChange={(e) => setHijoId(e.target.value)} className={inputClasses}>
                  {hijos.map((h) => (
                    <option key={h.id} value={h.id}>
                      {h.alumnoNombre}
                    </option>
                  ))}
                </select>
              </label>
            )}

            <div className="mt-4 grid grid-cols-2 gap-3">
              <label className="block text-sm font-semibold text-slate-700 dark:text-slate-300">
                Día
                <select
                  value={diaCumple}
                  onChange={(e) => setDiaCumple(Number(e.target.value))}
                  className={inputClasses}
                >
                  {Array.from({ length: diasDelMes }, (_, i) => i + 1).map((d) => (
                    <option key={d} value={d}>
                      {d}
                    </option>
                  ))}
                </select>
              </label>
              <label className="block text-sm font-semibold text-slate-700 dark:text-slate-300">
                Mes
                <select
                  value={mesCumple}
                  onChange={(e) => handleCambiarMes(Number(e.target.value))}
                  className={inputClasses}
                >
                  {nombresMeses.map((nombre, i) => (
                    <option key={nombre} value={i + 1}>
                      {nombre}
                    </option>
                  ))}
                </select>
              </label>
            </div>
            <p className="mt-2 text-xs text-slate-500 dark:text-slate-400">
              Nunca se pide ni se guarda el año de nacimiento.
            </p>
          </>
        ) : (
          <>
            {tipo === 'otros' && (
              <fieldset className="mt-4">
                <legend className="text-sm font-semibold text-slate-700 dark:text-slate-300">Alcance</legend>
                <div className="mt-1.5 flex gap-2">
                  <button
                    type="button"
                    onClick={() => setAlcance('clase')}
                    className={`flex-1 rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                      alcance === 'clase'
                        ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/20 dark:text-brand-300'
                        : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
                    }`}
                  >
                    Su clase
                  </button>
                  <button
                    type="button"
                    onClick={() => setAlcance('colegio')}
                    className={`flex-1 rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                      alcance === 'colegio'
                        ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/20 dark:text-brand-300'
                        : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
                    }`}
                  >
                    Todo el colegio
                  </button>
                </div>
              </fieldset>
            )}

            <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
              Título
              <input
                value={titulo}
                onChange={(e) => setTitulo(e.target.value)}
                maxLength={80}
                required
                className={inputClasses}
                placeholder="Ej. Excursión al museo"
              />
            </label>

            <div className="mt-4 grid grid-cols-2 gap-3">
              <label className="block text-sm font-semibold text-slate-700 dark:text-slate-300">
                Fecha
                <input
                  type="date"
                  value={fecha}
                  onChange={(e) => setFecha(e.target.value)}
                  required
                  className={inputClasses}
                />
              </label>
              <label className="block text-sm font-semibold text-slate-700 dark:text-slate-300">
                Hora (opcional)
                <input type="time" value={hora} onChange={(e) => setHora(e.target.value)} className={inputClasses} />
              </label>
            </div>

            <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
              Ubicación (opcional)
              <input
                value={ubicacion}
                onChange={(e) => setUbicacion(e.target.value)}
                className={inputClasses}
                placeholder="Ej. Museo de la Ciudad"
              />
            </label>

            <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
              Coste (opcional)
              <input
                value={coste}
                onChange={(e) => setCoste(e.target.value)}
                className={inputClasses}
                placeholder="Ej. 5 € por alumno"
              />
            </label>
          </>
        )}

        <button
          type="submit"
          disabled={!puedeEnviar}
          className="mt-6 w-full rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 transition-opacity disabled:cursor-not-allowed disabled:opacity-50"
        >
          {esCumpleanos ? 'Añadir cumpleaños' : 'Publicar evento'}
        </button>
      </form>
    </div>
  );
}

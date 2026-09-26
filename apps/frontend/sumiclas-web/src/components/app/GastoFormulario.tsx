import { useRef, useState, type FormEvent, type MouseEvent } from 'react';
import { X } from 'lucide-react';
import { ParticipantesDropdown } from '@/components/app/ParticipantesDropdown';
import { SelectDropdown } from '@/components/app/SelectDropdown';
import type { EventoRelacionado } from '@/data/calendarioMock';

export type NuevoGasto = {
  eventoId: string | null;
  concepto: string;
  importeTotal: number;
  participantesIds: string[];
};

const inputClasses =
  'mt-1.5 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white';

export function GastoFormulario({
  claseNombre,
  eventos,
  participantesDisponibles,
  onSubmit,
  onClose,
}: {
  claseNombre: string;
  eventos: EventoRelacionado[];
  participantesDisponibles: { id: string; nombre: string }[];
  onSubmit: (data: NuevoGasto) => void;
  onClose: () => void;
}) {
  const [eventoId, setEventoId] = useState('');
  const [concepto, setConcepto] = useState('');
  const [importePorFamilia, setImportePorFamilia] = useState('');
  const [participantesIds, setParticipantesIds] = useState<Set<string>>(
    new Set(participantesDisponibles.map((p) => p.id))
  );
  const mouseDownEnFondo = useRef(false);

  const puedeEnviar =
    concepto.trim().length > 0 && Number(importePorFamilia) > 0 && participantesIds.size > 0;
  const importeTotalCalculado = (Number(importePorFamilia) || 0) * participantesIds.size;

  const handleEventoChange = (id: string) => {
    setEventoId(id);
    const evento = eventos.find((ev) => ev.id === id);
    if (evento) setConcepto(evento.titulo);
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!puedeEnviar) return;
    onSubmit({
      eventoId: eventoId || null,
      concepto: concepto.trim(),
      importeTotal: importeTotalCalculado,
      participantesIds: [...participantesIds],
    });
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
          <h2 className="font-display text-xl font-extrabold text-slate-900 dark:text-white">Nuevo gasto</h2>
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

        {eventos.length > 0 && (
          <div className="mt-5">
            <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">
              Evento relacionado (opcional)
            </span>
            <div className="mt-1.5">
              <SelectDropdown
                value={eventoId}
                placeholder="Ninguno"
                options={[{ value: '', label: 'Ninguno' }, ...eventos.map((ev) => ({ value: ev.id, label: ev.titulo }))]}
                onChange={handleEventoChange}
              />
            </div>
          </div>
        )}

        <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
          Concepto
          <input
            value={concepto}
            onChange={(e) => setConcepto(e.target.value)}
            maxLength={80}
            required
            className={inputClasses}
            placeholder="Ej. Regalo de fin de curso"
          />
        </label>

        <div className="mt-4">
          <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">Participantes</span>
          <div className="mt-1.5">
            <ParticipantesDropdown
              participantesDisponibles={participantesDisponibles}
              seleccionados={participantesIds}
              onCambiar={setParticipantesIds}
            />
          </div>
        </div>

        <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
          Importe por familia (€)
          <input
            type="number"
            min="0"
            step="0.01"
            value={importePorFamilia}
            onChange={(e) => setImportePorFamilia(e.target.value)}
            required
            className={inputClasses}
            placeholder="Ej. 5"
          />
        </label>
        <p className="mt-1.5 text-xs text-slate-500 dark:text-slate-400">
          Total: {importeTotalCalculado.toFixed(2)} € entre {participantesIds.size}{' '}
          {participantesIds.size === 1 ? 'familia' : 'familias'}
        </p>

        <button
          type="submit"
          disabled={!puedeEnviar}
          className="mt-6 w-full rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 transition-opacity disabled:cursor-not-allowed disabled:opacity-50"
        >
          Publicar gasto
        </button>
      </form>
    </div>
  );
}

import { useState, type FormEvent } from 'react';
import { UserRound, ArrowRight, Users2 } from 'lucide-react';
import type { AlumnoRegistrado } from '@/data/onboardingMock';
import { normalizeText } from '@/lib/text';
import type { TutorRole } from '@/lib/auth';

type ConfirmPayload = {
  nombre: string;
  apellido: string;
  tutorNombre: string;
  tutorRol: TutorRole;
  unirseComoSegundoTutor: boolean;
  alumnoExistenteId?: string;
};

const rolOptions: { value: TutorRole; label: string }[] = [
  { value: 'madre', label: 'Madre' },
  { value: 'padre', label: 'Padre' },
  { value: 'tutor', label: 'Tutor/a legal' },
];

export function StudentStep({
  alumnosEnClase,
  onConfirm,
}: {
  alumnosEnClase: AlumnoRegistrado[];
  onConfirm: (data: ConfirmPayload) => void;
}) {
  const [nombre, setNombre] = useState('');
  const [apellido, setApellido] = useState('');
  const [tutorNombre, setTutorNombre] = useState('');
  const [tutorRol, setTutorRol] = useState<TutorRole | null>(null);
  const [posiblesCoincidencias, setPosiblesCoincidencias] = useState<AlumnoRegistrado[] | null>(null);

  const buscarCoincidencias = () => {
    const n = normalizeText(nombre);
    const a = normalizeText(apellido);
    return alumnosEnClase.filter(
      (al) => normalizeText(al.nombre) === n && normalizeText(al.apellido).charAt(0) === a.charAt(0)
    );
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!nombre.trim() || !apellido.trim() || !tutorNombre.trim() || !tutorRol) return;
    const matches = buscarCoincidencias();
    if (matches.length > 0) {
      setPosiblesCoincidencias(matches);
      return;
    }
    onConfirm({
      nombre: nombre.trim(),
      apellido: apellido.trim(),
      tutorNombre: tutorNombre.trim(),
      tutorRol,
      unirseComoSegundoTutor: false,
    });
  };

  if (posiblesCoincidencias) {
    return (
      <div className="space-y-3">
        {posiblesCoincidencias.map((al) => (
          <div
            key={al.id}
            className="flex flex-col gap-3 rounded-2xl border border-sun-200 bg-sun-50/60 p-4 dark:border-sun-700/40 dark:bg-sun-900/15 sm:flex-row sm:items-center sm:justify-between"
          >
            <div className="flex items-center gap-3">
              <div className="grid h-10 w-10 shrink-0 place-items-center rounded-full bg-sun-100 font-display text-sm font-bold text-sun-700 dark:bg-sun-900/40 dark:text-sun-300">
                {al.nombre.charAt(0)}
                {al.apellido.charAt(0)}
              </div>
              <p className="text-sm text-slate-700 dark:text-slate-300">
                Ya hay un alumno llamado{' '}
                <strong className="font-semibold text-slate-900 dark:text-white">
                  {al.nombre} {al.apellido.charAt(0)}.
                </strong>{' '}
                en esta clase. ¿Es tu hijo?
              </p>
            </div>
            <button
              type="button"
              onClick={() =>
                onConfirm({
                  nombre,
                  apellido,
                  tutorNombre: tutorNombre.trim(),
                  tutorRol: tutorRol as TutorRole,
                  unirseComoSegundoTutor: true,
                  alumnoExistenteId: al.id,
                })
              }
              className="inline-flex shrink-0 items-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-3.5 py-2 text-sm font-semibold text-white shadow-sm shadow-brand-500/25"
            >
              <Users2 className="h-4 w-4" />
              Sí, únete como tutor
            </button>
          </div>
        ))}
        <button
          type="button"
          onClick={() =>
            onConfirm({
              nombre: nombre.trim(),
              apellido: apellido.trim(),
              tutorNombre: tutorNombre.trim(),
              tutorRol: tutorRol as TutorRole,
              unirseComoSegundoTutor: false,
            })
          }
          className="text-sm font-semibold text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200"
        >
          No, es otro niño — crear como alumno nuevo
        </button>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label
          htmlFor="nombre-alumno"
          className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300"
        >
          Nombre del alumno/a
        </label>
        <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
          <UserRound className="h-4 w-4 shrink-0 text-slate-400" />
          <input
            id="nombre-alumno"
            type="text"
            required
            value={nombre}
            onChange={(e) => setNombre(e.target.value)}
            placeholder="Martina"
            className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
          />
        </div>
      </div>

      <div>
        <label
          htmlFor="apellido-alumno"
          className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300"
        >
          Primer apellido
        </label>
        <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
          <UserRound className="h-4 w-4 shrink-0 text-slate-400" />
          <input
            id="apellido-alumno"
            type="text"
            required
            value={apellido}
            onChange={(e) => setApellido(e.target.value)}
            placeholder="Ruiz"
            className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
          />
        </div>
        <p className="mt-1.5 text-xs text-slate-500 dark:text-slate-400">
          Solo se mostrará a otras familias como "{nombre || 'Nombre'} {apellido ? `${apellido.charAt(0)}.` : 'A.'}"
        </p>
      </div>

      <div className="border-t border-slate-200 pt-4 dark:border-slate-700">
        <p className="mb-3 text-sm font-semibold text-slate-700 dark:text-slate-300">Tus datos</p>

        <div>
          <label
            htmlFor="nombre-tutor"
            className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300"
          >
            Tu nombre
          </label>
          <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
            <UserRound className="h-4 w-4 shrink-0 text-slate-400" />
            <input
              id="nombre-tutor"
              type="text"
              required
              value={tutorNombre}
              onChange={(e) => setTutorNombre(e.target.value)}
              placeholder="Ana Ruiz"
              className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
            />
          </div>
        </div>

        <div className="mt-4">
          <p className="mb-1.5 text-sm font-semibold text-slate-700 dark:text-slate-300">Eres su...</p>
          <div className="grid grid-cols-3 gap-2">
            {rolOptions.map((opt) => (
              <button
                key={opt.value}
                type="button"
                onClick={() => setTutorRol(opt.value)}
                className={`rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                  tutorRol === opt.value
                    ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/30 dark:text-brand-300'
                    : 'border-slate-200 bg-slate-50 text-slate-600 hover:border-slate-300 dark:border-slate-700 dark:bg-slate-800/60 dark:text-slate-300 dark:hover:border-slate-600'
                }`}
              >
                {opt.label}
              </button>
            ))}
          </div>
        </div>
      </div>

      <button
        type="submit"
        className="mt-2 flex w-full items-center justify-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-brand-500/25 transition-transform hover:scale-[1.02] active:scale-95"
      >
        Añadir alumno y entrar a Avisos
        <ArrowRight className="h-4 w-4" />
      </button>
    </form>
  );
}

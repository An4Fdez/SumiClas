import { useState, type FormEvent } from 'react';
import { UserRound, ArrowRight, Users2 } from 'lucide-react';
import type { RegisteredStudent } from '@/data/onboardingMock';
import { normalizeText } from '@/lib/text';

type ConfirmPayload = {
  name: string;
  lastName: string;
  joinAsSecondGuardian: boolean;
  existingStudentId?: string;
};

export function StudentStep({
  studentsInClass,
  onConfirm,
}: {
  studentsInClass: RegisteredStudent[];
  onConfirm: (data: ConfirmPayload) => void;
}) {
  const [name, setName] = useState('');
  const [lastName, setLastName] = useState('');
  const [possibleMatches, setPossibleMatches] = useState<RegisteredStudent[] | null>(null);

  const findMatches = () => {
    const n = normalizeText(name);
    const a = normalizeText(lastName);
    return studentsInClass.filter(
      (st) => normalizeText(st.name) === n && normalizeText(st.lastName).charAt(0) === a.charAt(0)
    );
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!name.trim() || !lastName.trim()) return;
    const matches = findMatches();
    if (matches.length > 0) {
      setPossibleMatches(matches);
      return;
    }
    onConfirm({ name: name.trim(), lastName: lastName.trim(), joinAsSecondGuardian: false });
  };

  if (possibleMatches) {
    return (
      <div className="space-y-3">
        {possibleMatches.map((st) => (
          <div
            key={st.id}
            className="flex flex-col gap-3 rounded-2xl border border-sun-200 bg-sun-50/60 p-4 dark:border-sun-700/40 dark:bg-sun-900/15 sm:flex-row sm:items-center sm:justify-between"
          >
            <div className="flex items-center gap-3">
              <div className="grid h-10 w-10 shrink-0 place-items-center rounded-full bg-sun-100 font-display text-sm font-bold text-sun-700 dark:bg-sun-900/40 dark:text-sun-300">
                {st.name.charAt(0)}
                {st.lastName.charAt(0)}
              </div>
              <p className="text-sm text-slate-700 dark:text-slate-300">
                Ya hay un alumno llamado{' '}
                <strong className="font-semibold text-slate-900 dark:text-white">
                  {st.name} {st.lastName.charAt(0)}.
                </strong>{' '}
                en esta clase. ¿Es tu hijo?
              </p>
            </div>
            <button
              type="button"
              onClick={() =>
                onConfirm({ name, lastName, joinAsSecondGuardian: true, existingStudentId: st.id })
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
            onConfirm({ name: name.trim(), lastName: lastName.trim(), joinAsSecondGuardian: false })
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
          htmlFor="student-name"
          className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300"
        >
          Nombre del alumno/a
        </label>
        <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
          <UserRound className="h-4 w-4 shrink-0 text-slate-400" />
          <input
            id="student-name"
            type="text"
            required
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Martina"
            className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
          />
        </div>
      </div>

      <div>
        <label
          htmlFor="student-last-name"
          className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300"
        >
          Primer apellido
        </label>
        <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
          <UserRound className="h-4 w-4 shrink-0 text-slate-400" />
          <input
            id="student-last-name"
            type="text"
            required
            value={lastName}
            onChange={(e) => setLastName(e.target.value)}
            placeholder="Ruiz"
            className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
          />
        </div>
        <p className="mt-1.5 text-xs text-slate-500 dark:text-slate-400">
          Solo se mostrará a otras familias como "{name || 'Nombre'} {lastName ? `${lastName.charAt(0)}.` : 'A.'}"
        </p>
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

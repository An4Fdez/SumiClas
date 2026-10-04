import { useState, type FormEvent } from 'react';
import { LayoutGrid, Plus, ArrowRight } from 'lucide-react';
import { getApiErrorMessage } from '@/lib/apiError';
import type { SchoolClass } from '@/modules/classes/domain/IClassRepository';

const cardColors = ['brand', 'mint', 'rose', 'sun', 'violet', 'orange'] as const;
type CardColor = (typeof cardColors)[number];

const colorClasses: Record<CardColor, string> = {
  brand:
    'border-brand-200 bg-brand-50/60 text-brand-700 hover:border-brand-300 dark:border-brand-700/40 dark:bg-brand-900/15 dark:text-brand-300',
  mint: 'border-mint-200 bg-mint-50/60 text-mint-700 hover:border-mint-300 dark:border-mint-700/40 dark:bg-mint-900/15 dark:text-mint-300',
  rose: 'border-rose-200 bg-rose-50/60 text-rose-700 hover:border-rose-300 dark:border-rose-700/40 dark:bg-rose-900/15 dark:text-rose-300',
  sun: 'border-sun-200 bg-sun-50/60 text-sun-700 hover:border-sun-300 dark:border-sun-700/40 dark:bg-sun-900/15 dark:text-sun-300',
  violet:
    'border-violet-200 bg-violet-50/60 text-violet-700 hover:border-violet-300 dark:border-violet-700/40 dark:bg-violet-900/15 dark:text-violet-300',
  orange:
    'border-orange-200 bg-orange-50/60 text-orange-700 hover:border-orange-300 dark:border-orange-700/40 dark:bg-orange-900/15 dark:text-orange-300',
};

export function ClassStep({
  schoolClasses,
  loading,
  error,
  onRetry,
  onSelect,
  onCreate,
}: {
  schoolClasses: SchoolClass[];
  loading: boolean;
  error: string | null;
  onRetry: () => void;
  onSelect: (schoolClass: SchoolClass) => void;
  onCreate: (name: string) => Promise<void>;
}) {
  const [creating, setCreating] = useState(false);
  const [gradeLevel, setGradeLevel] = useState('');
  const [letterSuffix, setLetterSuffix] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);

  const handleCreate = async (e: FormEvent) => {
    e.preventDefault();
    const trimmedGrade = gradeLevel.trim();
    const trimmedLetter = letterSuffix.trim().toUpperCase();
    if (!trimmedGrade || !trimmedLetter) return;
    setSubmitting(true);
    setCreateError(null);
    try {
      await onCreate(`${trimmedGrade}${trimmedLetter}`);
    } catch (err) {
      setCreateError(getApiErrorMessage(err));
      setSubmitting(false);
    }
  };

  if (loading) {
    return <p className="text-sm text-slate-500 dark:text-slate-400">Cargando las clases del colegio…</p>;
  }

  if (error) {
    return (
      <div role="alert" className="rounded-2xl border border-rose-200 bg-rose-50/60 p-4 dark:border-rose-700/40 dark:bg-rose-900/15">
        <p className="text-sm text-slate-700 dark:text-slate-300">{error}</p>
        <button
          type="button"
          onClick={onRetry}
          className="mt-3 text-sm font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
        >
          Reintentar
        </button>
      </div>
    );
  }

  return (
    <div>
      {schoolClasses.length > 0 ? (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
          {schoolClasses.map((c, i) => (
            <button
              key={c.id}
              type="button"
              onClick={() => onSelect(c)}
              className={`rounded-2xl border p-4 text-center transition-all hover:-translate-y-0.5 hover:shadow-md ${
                colorClasses[cardColors[i % cardColors.length]]
              }`}
            >
              <span className="font-display text-lg font-bold">{c.name}</span>
            </button>
          ))}
        </div>
      ) : (
        <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 p-6 text-center dark:border-slate-700 dark:bg-slate-900/60">
          <LayoutGrid className="mx-auto h-8 w-8 text-slate-300 dark:text-slate-600" />
          <p className="mt-3 text-sm text-slate-600 dark:text-slate-400">
            Todavía no hay ninguna clase creada en este colegio para el curso activo.
          </p>
        </div>
      )}

      {!creating ? (
        <button
          type="button"
          onClick={() => setCreating(true)}
          className="mt-4 flex w-full items-center justify-center gap-2 rounded-2xl border border-dashed border-slate-300 px-4 py-3.5 text-sm font-semibold text-slate-600 transition-colors hover:border-brand-300 hover:bg-brand-50/50 hover:text-brand-700 dark:border-slate-700 dark:text-slate-400 dark:hover:border-brand-700/50 dark:hover:bg-brand-900/15 dark:hover:text-brand-300"
        >
          <Plus className="h-4 w-4" />
          Mi clase no está aquí, quiero crearla
        </button>
      ) : (
        <form
          onSubmit={handleCreate}
          className="mt-4 rounded-2xl border border-brand-200 bg-brand-50/40 p-4 dark:border-brand-700/40 dark:bg-brand-900/15 sm:p-5"
        >
          <p className="text-sm font-semibold text-slate-900 dark:text-white">Crear nueva clase</p>
          <p className="mt-1 text-xs text-slate-600 dark:text-slate-400">
            Serás el delegado provisional de esta clase — podrás ceder el rol más adelante.
          </p>
          <div className="mt-3 flex gap-2">
            <input
              type="text"
              value={gradeLevel}
              onChange={(e) => setGradeLevel(e.target.value)}
              placeholder="Curso (ej. 3º)"
              required
              className="w-2/3 rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-sm text-slate-700 placeholder:text-slate-400 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-200"
            />
            <input
              type="text"
              value={letterSuffix}
              onChange={(e) => setLetterSuffix(e.target.value)}
              placeholder="Letra (ej. C)"
              maxLength={2}
              required
              className="w-1/3 rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-sm text-slate-700 placeholder:text-slate-400 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-200"
            />
          </div>
          {createError && (
            <p role="alert" className="mt-2 text-xs font-semibold text-rose-600 dark:text-rose-400">
              {createError}
            </p>
          )}
          <div className="mt-3 flex items-center gap-2">
            <button
              type="submit"
              disabled={submitting}
              className="inline-flex items-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-md shadow-brand-500/25 transition-transform hover:scale-[1.02] active:scale-95 disabled:opacity-60"
            >
              {submitting ? 'Creando…' : 'Crear clase'}
              <ArrowRight className="h-4 w-4" />
            </button>
            <button
              type="button"
              onClick={() => setCreating(false)}
              className="rounded-xl px-3 py-2.5 text-sm font-semibold text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200"
            >
              Cancelar
            </button>
          </div>
        </form>
      )}
    </div>
  );
}

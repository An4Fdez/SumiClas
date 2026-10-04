import { useEffect, useState, type FormEvent, type KeyboardEvent } from 'react';
import { Search, ChevronRight, Plus, ArrowRight } from 'lucide-react';
import { useOnboardingRepositories } from '@/hooks/useOnboardingRepositories';
import { getApiErrorMessage } from '@/lib/apiError';
import type { NewSchool, School } from '@/modules/schools/domain/ISchoolRepository';

const MAX_DROPDOWN_RESULTS = 8;
const DEBOUNCE_MS = 300;

const inputClasses =
  'w-full rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-sm text-slate-700 placeholder:text-slate-400 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-200';

export function SchoolSearchStep({
  onSelect,
  onRegister,
}: {
  onSelect: (school: School) => void;
  onRegister: (school: Omit<NewSchool, 'id'>) => Promise<void>;
}) {
  const { schools } = useOnboardingRepositories();
  const [query, setQuery] = useState('');
  const [debouncedQuery, setDebouncedQuery] = useState('');
  const [results, setResults] = useState<School[]>([]);
  const [searching, setSearching] = useState(false);
  const [searchError, setSearchError] = useState<string | null>(null);
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const [showFullList, setShowFullList] = useState(false);
  const [registering, setRegistering] = useState(false);
  const [officialCode, setOfficialCode] = useState('');
  const [address, setAddress] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  useEffect(() => {
    const t = setTimeout(() => setDebouncedQuery(query), DEBOUNCE_MS);
    return () => clearTimeout(t);
  }, [query]);

  const trimmed = debouncedQuery.trim();

  useEffect(() => {
    if (!trimmed) {
      setResults([]);
      setSearchError(null);
      return;
    }
    let cancelled = false;
    setSearching(true);
    schools
      .searchByName(trimmed)
      .then((found) => {
        if (cancelled) return;
        setResults(found);
        setSearchError(null);
      })
      .catch((error: unknown) => {
        if (cancelled) return;
        setResults([]);
        setSearchError(getApiErrorMessage(error));
      })
      .finally(() => {
        if (!cancelled) setSearching(false);
      });
    return () => {
      cancelled = true;
    };
  }, [trimmed, schools]);

  const handleSelect = (school: School) => {
    onSelect(school);
    setDropdownOpen(false);
    setShowFullList(false);
    setQuery('');
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && query.trim().length > 0) {
      e.preventDefault();
      setDebouncedQuery(query);
      setDropdownOpen(false);
      setShowFullList(true);
    }
  };

  const handleRegister = async (e: FormEvent) => {
    e.preventDefault();
    const name = query.trim();
    if (!name || !officialCode.trim()) return;
    setSubmitting(true);
    setFormError(null);
    try {
      await onRegister({ name, officialCode: officialCode.trim(), address: address.trim() || undefined });
    } catch (error) {
      setFormError(getApiErrorMessage(error));
      setSubmitting(false);
    }
  };

  const dropdownVisible = dropdownOpen && !showFullList && trimmed.length > 0;
  const emptyMessage = searchError ?? (searching ? 'Buscando…' : `Sin resultados para "${trimmed}".`);

  return (
    <div>
      <div className="relative">
        <div className="flex items-center gap-2 rounded-2xl border border-slate-200 bg-white p-2 shadow-sm dark:border-slate-700 dark:bg-slate-900">
          <Search className="ml-2 h-5 w-5 shrink-0 text-slate-400" />
          <input
            type="text"
            value={query}
            maxLength={200}
            onChange={(e) => {
              setQuery(e.target.value);
              setDropdownOpen(true);
              setShowFullList(false);
            }}
            onKeyDown={handleKeyDown}
            onFocus={() => query.trim().length > 0 && setDropdownOpen(true)}
            onBlur={() => setTimeout(() => setDropdownOpen(false), 120)}
            placeholder="Escribe el nombre de tu colegio…"
            className="w-full bg-transparent px-1 py-1.5 text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
          />
        </div>

        {dropdownVisible && (
          <div className="absolute inset-x-0 top-full z-30 mt-2 max-h-[300px] overflow-auto rounded-2xl border border-slate-200 bg-white p-1.5 shadow-xl dark:border-slate-700 dark:bg-slate-900">
            {results.length === 0 ? (
              <p className="px-3 py-3 text-sm text-slate-500 dark:text-slate-400">{emptyMessage}</p>
            ) : (
              results.slice(0, MAX_DROPDOWN_RESULTS).map((s) => (
                <button
                  key={s.id}
                  type="button"
                  onClick={() => handleSelect(s)}
                  className="flex w-full items-center justify-between gap-2 rounded-xl px-3 py-2.5 text-left transition-colors hover:bg-slate-50 dark:hover:bg-slate-800"
                >
                  <span>
                    <span className="block text-sm font-semibold text-slate-900 dark:text-white">{s.name}</span>
                    {s.address && <span className="block text-xs text-slate-500 dark:text-slate-400">{s.address}</span>}
                  </span>
                  <ChevronRight className="h-4 w-4 shrink-0 text-slate-300 dark:text-slate-600" />
                </button>
              ))
            )}
          </div>
        )}
      </div>

      {showFullList && (
        <div className="mt-4 rounded-2xl border border-slate-200 bg-white p-2 dark:border-slate-700 dark:bg-slate-900">
          <p className="px-3 pt-2 text-xs font-semibold uppercase tracking-wider text-slate-400 dark:text-slate-500">
            {results.length} {results.length === 1 ? 'resultado' : 'resultados'} para "{trimmed}"
          </p>
          <div className="mt-1 divide-y divide-slate-100 dark:divide-slate-800">
            {results.length === 0 ? (
              <p className="px-3 py-4 text-sm text-slate-500 dark:text-slate-400">
                {searchError ?? (searching ? 'Buscando…' : 'No hemos encontrado ningún colegio. Prueba con otro nombre o regístralo.')}
              </p>
            ) : (
              results.map((s) => (
                <button
                  key={s.id}
                  type="button"
                  onClick={() => handleSelect(s)}
                  className="flex w-full items-center justify-between gap-2 px-3 py-3 text-left transition-colors hover:bg-slate-50 dark:hover:bg-slate-800"
                >
                  <span>
                    <span className="block text-sm font-semibold text-slate-900 dark:text-white">{s.name}</span>
                    {s.address && <span className="block text-xs text-slate-500 dark:text-slate-400">{s.address}</span>}
                  </span>
                  <ChevronRight className="h-4 w-4 shrink-0 text-slate-300 dark:text-slate-600" />
                </button>
              ))
            )}
          </div>
        </div>
      )}

      {!registering ? (
        <button
          type="button"
          onClick={() => setRegistering(true)}
          className="mt-4 flex w-full items-center justify-center gap-2 rounded-2xl border border-dashed border-slate-300 px-4 py-3.5 text-sm font-semibold text-slate-600 transition-colors hover:border-brand-300 hover:bg-brand-50/50 hover:text-brand-700 dark:border-slate-700 dark:text-slate-400 dark:hover:border-brand-700/50 dark:hover:bg-brand-900/15 dark:hover:text-brand-300"
        >
          <Plus className="h-4 w-4" />
          Mi colegio no aparece, quiero registrarlo
        </button>
      ) : (
        <form
          onSubmit={handleRegister}
          className="mt-4 rounded-2xl border border-brand-200 bg-brand-50/40 p-4 dark:border-brand-700/40 dark:bg-brand-900/15 sm:p-5"
        >
          <p className="text-sm font-semibold text-slate-900 dark:text-white">Registrar colegio</p>
          <p className="mt-1 text-xs text-slate-600 dark:text-slate-400">
            Usaremos el nombre que has escrito arriba. Añade el código oficial del centro, que figura en el listado del
            Ministerio de Educación.
          </p>
          <div className="mt-3 space-y-2">
            <input
              type="text"
              value={officialCode}
              onChange={(e) => setOfficialCode(e.target.value)}
              placeholder="Código oficial del centro"
              maxLength={50}
              required
              className={inputClasses}
            />
            <input
              type="text"
              value={address}
              onChange={(e) => setAddress(e.target.value)}
              placeholder="Dirección (opcional)"
              maxLength={300}
              className={inputClasses}
            />
          </div>
          {!query.trim() && (
            <p className="mt-2 text-xs text-slate-500 dark:text-slate-400">Escribe arriba el nombre del colegio.</p>
          )}
          {formError && (
            <p role="alert" className="mt-2 text-xs font-semibold text-rose-600 dark:text-rose-400">
              {formError}
            </p>
          )}
          <div className="mt-3 flex items-center gap-2">
            <button
              type="submit"
              disabled={submitting || !query.trim()}
              className="inline-flex items-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-md shadow-brand-500/25 transition-transform hover:scale-[1.02] active:scale-95 disabled:opacity-60"
            >
              {submitting ? 'Registrando…' : 'Registrar colegio'}
              <ArrowRight className="h-4 w-4" />
            </button>
            <button
              type="button"
              onClick={() => setRegistering(false)}
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

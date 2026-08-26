import { useEffect, useState, type KeyboardEvent } from 'react';
import { Search, ChevronRight } from 'lucide-react';
import type { Colegio } from '@/data/onboardingMock';
import { normalizeText } from '@/lib/text';

const MAX_DROPDOWN_RESULTS = 8;
const DEBOUNCE_MS = 300;

export function SchoolSearchStep({
  colegios,
  onSelect,
}: {
  colegios: Colegio[];
  onSelect: (colegio: Colegio) => void;
}) {
  const [query, setQuery] = useState('');
  const [debouncedQuery, setDebouncedQuery] = useState('');
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const [showFullList, setShowFullList] = useState(false);

  useEffect(() => {
    const t = setTimeout(() => setDebouncedQuery(query), DEBOUNCE_MS);
    return () => clearTimeout(t);
  }, [query]);

  const trimmed = debouncedQuery.trim();
  const results =
    trimmed.length > 0
      ? colegios.filter((c) => {
          const q = normalizeText(trimmed);
          return normalizeText(c.nombre).includes(q) || normalizeText(c.localidad).includes(q);
        })
      : [];

  const handleSelect = (colegio: Colegio) => {
    onSelect(colegio);
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

  const dropdownVisible = dropdownOpen && !showFullList && trimmed.length > 0;

  return (
    <div>
      <div className="relative">
        <div className="flex items-center gap-2 rounded-2xl border border-slate-200 bg-white p-2 shadow-sm dark:border-slate-700 dark:bg-slate-900">
          <Search className="ml-2 h-5 w-5 shrink-0 text-slate-400" />
          <input
            type="text"
            value={query}
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
              <p className="px-3 py-3 text-sm text-slate-500 dark:text-slate-400">
                Sin resultados para "{trimmed}".
              </p>
            ) : (
              results.slice(0, MAX_DROPDOWN_RESULTS).map((c) => (
                <button
                  key={c.id}
                  type="button"
                  onClick={() => handleSelect(c)}
                  className="flex w-full items-center justify-between gap-2 rounded-xl px-3 py-2.5 text-left transition-colors hover:bg-slate-50 dark:hover:bg-slate-800"
                >
                  <span>
                    <span className="block text-sm font-semibold text-slate-900 dark:text-white">{c.nombre}</span>
                    <span className="block text-xs text-slate-500 dark:text-slate-400">{c.localidad}</span>
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
                No hemos encontrado ningún colegio. Prueba con otro nombre o localidad.
              </p>
            ) : (
              results.map((c) => (
                <button
                  key={c.id}
                  type="button"
                  onClick={() => handleSelect(c)}
                  className="flex w-full items-center justify-between gap-2 px-3 py-3 text-left transition-colors hover:bg-slate-50 dark:hover:bg-slate-800"
                >
                  <span>
                    <span className="block text-sm font-semibold text-slate-900 dark:text-white">{c.nombre}</span>
                    <span className="block text-xs text-slate-500 dark:text-slate-400">
                      {c.direccion}, {c.localidad}
                    </span>
                  </span>
                  <ChevronRight className="h-4 w-4 shrink-0 text-slate-300 dark:text-slate-600" />
                </button>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}

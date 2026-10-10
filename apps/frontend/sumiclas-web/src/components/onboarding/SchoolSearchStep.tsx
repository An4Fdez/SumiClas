import { useMemo, useState } from 'react';
import { Search, ChevronRight } from 'lucide-react';
import { SearchableSelect } from '@/components/app/SearchableSelect';
import { useLocalitySchools } from '@/hooks/useLocalitySchools';
import { useProvinceLocalities } from '@/hooks/useProvinceLocalities';
import { useProvinces } from '@/hooks/useProvinces';
import type { School } from '@/modules/schools/domain/School';

const MAX_RESULTS = 50;

const messageClasses = 'px-3 py-4 text-sm text-slate-500 dark:text-slate-400';

export function SchoolSearchStep({ onSelect }: { onSelect: (school: School) => void }) {
  const [provinceId, setProvinceId] = useState('');
  const [localityId, setLocalityId] = useState('');
  const [name, setName] = useState('');
  const { state: provincesState, reload: reloadProvinces } = useProvinces();
  const { state: localitiesState, reload: reloadLocalities } = useProvinceLocalities(provinceId || undefined);
  const { state: schoolsState, reload: reloadSchools, searching } = useLocalitySchools(localityId || undefined, name);

  const provinceOptions = useMemo(
    () =>
      provincesState.status === 'ready' ? provincesState.provinces.map((p) => ({ value: p.id, label: p.name })) : [],
    [provincesState],
  );
  const localityOptions = useMemo(
    () =>
      localitiesState.status === 'ready' ? localitiesState.localities.map((l) => ({ value: l.id, label: l.name })) : [],
    [localitiesState],
  );

  const handleProvinceChange = (id: string) => {
    if (id === provinceId) return;
    setProvinceId(id);
    setLocalityId('');
    setName('');
  };

  const handleLocalityChange = (id: string) => {
    if (id === localityId) return;
    setLocalityId(id);
    setName('');
  };

  const trimmedName = name.trim();

  return (
    <div>
      <div className="grid gap-3 sm:grid-cols-2">
        <div>
          <p className="mb-1.5 text-xs font-semibold uppercase tracking-wider text-slate-400 dark:text-slate-500">
            Provincia
          </p>
          <SearchableSelect
            ariaLabel="Provincia"
            value={provinceId}
            options={provinceOptions}
            placeholder="Elige tu provincia"
            loading={provincesState.status === 'loading'}
            disabled={provincesState.status !== 'ready'}
            onChange={handleProvinceChange}
          />
          {provincesState.status === 'error' && (
            <ErrorMessage message={provincesState.message} onRetry={reloadProvinces} />
          )}
        </div>
        <div>
          <p className="mb-1.5 text-xs font-semibold uppercase tracking-wider text-slate-400 dark:text-slate-500">
            Población
          </p>
          <SearchableSelect
            ariaLabel="Población"
            value={localityId}
            options={localityOptions}
            placeholder={provinceId ? 'Elige tu población' : 'Elige antes la provincia'}
            loading={localitiesState.status === 'loading'}
            disabled={localitiesState.status !== 'ready'}
            onChange={handleLocalityChange}
          />
          {localitiesState.status === 'error' && (
            <ErrorMessage message={localitiesState.message} onRetry={reloadLocalities} />
          )}
        </div>
      </div>

      {localityId && (
        <div className="mt-4">
          <div className="flex items-center gap-2 rounded-2xl border border-slate-200 bg-white p-2 shadow-sm dark:border-slate-700 dark:bg-slate-900">
            <Search className="ml-2 h-5 w-5 shrink-0 text-slate-400" />
            <input
              type="text"
              value={name}
              maxLength={200}
              onChange={(e) => setName(e.target.value)}
              placeholder="Escribe parte del nombre del colegio…"
              aria-label="Nombre del colegio"
              className="w-full bg-transparent px-1 py-1.5 text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
            />
          </div>

          <div className="mt-3 rounded-2xl border border-slate-200 bg-white p-2 dark:border-slate-700 dark:bg-slate-900">
            {schoolsState.status === 'error' ? (
              <div className="px-3 py-2">
                <ErrorMessage message={schoolsState.message} onRetry={reloadSchools} />
              </div>
            ) : schoolsState.status !== 'ready' ? (
              <p className={messageClasses}>Buscando colegios…</p>
            ) : schoolsState.schools.length === 0 ? (
              <p className={messageClasses}>
                {trimmedName
                  ? `No hay ningún colegio de esta población que coincida con "${trimmedName}".`
                  : 'No tenemos colegios de esta población en el listado oficial.'}
              </p>
            ) : (
              <>
                <p className="px-3 pt-2 text-xs font-semibold uppercase tracking-wider text-slate-400 dark:text-slate-500">
                  {searching ? 'Buscando…' : resultsLabel(schoolsState.schools.length)}
                </p>
                <div className="mt-1 divide-y divide-slate-100 dark:divide-slate-800">
                  {schoolsState.schools.map((s) => (
                    <button
                      key={s.id}
                      type="button"
                      onClick={() => onSelect(s)}
                      className="flex w-full items-center justify-between gap-2 px-3 py-3 text-left transition-colors hover:bg-slate-50 dark:hover:bg-slate-800"
                    >
                      <span className="min-w-0">
                        <span className="block text-sm font-semibold text-slate-900 dark:text-white">{s.name}</span>
                        {schoolLocation(s) && (
                          <span className="block text-xs text-slate-500 dark:text-slate-400">{schoolLocation(s)}</span>
                        )}
                        {s.phone && (
                          <span className="block text-xs text-slate-400 dark:text-slate-500">Tel. {s.phone}</span>
                        )}
                      </span>
                      <ChevronRight className="h-4 w-4 shrink-0 text-slate-300 dark:text-slate-600" />
                    </button>
                  ))}
                </div>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

function resultsLabel(count: number) {
  if (count >= MAX_RESULTS) return `Mostrando los primeros ${MAX_RESULTS}. Escribe el nombre para acotar.`;
  return `${count} ${count === 1 ? 'resultado' : 'resultados'}`;
}

function schoolLocation(school: School) {
  const place = [school.postalCode, school.localityName].filter(Boolean).join(' ');
  return [school.address, place].filter(Boolean).join(' · ');
}

function ErrorMessage({ message, onRetry }: { message: string; onRetry: () => void }) {
  return (
    <p role="alert" className="mt-1.5 text-xs font-semibold text-rose-600 dark:text-rose-400">
      {message}{' '}
      <button type="button" onClick={onRetry} className="underline">
        Reintentar
      </button>
    </p>
  );
}

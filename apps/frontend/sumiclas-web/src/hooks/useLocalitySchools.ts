import { useCallback, useEffect, useRef, useState } from 'react';
import { useOnboardingRepositories } from '@/hooks/useOnboardingRepositories';
import { getApiErrorMessage } from '@/lib/apiError';
import type { School } from '@/modules/schools/domain/School';

const DEBOUNCE_MS = 300;

type State =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'ready'; schools: School[] }
  | { status: 'error'; message: string };

/**
 * The schools of one locality filtered by name (the API returns at most 50). The name is debounced; the first
 * request of a locality is sent at once. `generation` keeps a late response or error of a previous locality or
 * name from reaching the current one. Pass `undefined` to skip the request.
 */
export function useLocalitySchools(localityId: string | undefined, name: string) {
  const { schools } = useOnboardingRepositories();
  const [state, setState] = useState<State>({ status: 'idle' });
  const [debouncedName, setDebouncedName] = useState(name);
  const [reloadKey, setReloadKey] = useState(0);
  const generation = useRef(0);

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedName(name), DEBOUNCE_MS);
    return () => clearTimeout(timer);
  }, [name]);

  const trimmed = debouncedName.trim();

  useEffect(() => {
    const mine = ++generation.current;
    if (!localityId) {
      setState({ status: 'idle' });
      return;
    }
    setState({ status: 'loading' });
    schools
      .findByLocality(localityId, trimmed || undefined)
      .then((found) => {
        if (generation.current === mine) setState({ status: 'ready', schools: found });
      })
      .catch((error: unknown) => {
        if (generation.current === mine) setState({ status: 'error', message: getApiErrorMessage(error) });
      });
    return () => {
      generation.current += 1;
    };
  }, [localityId, trimmed, schools, reloadKey]);

  const reload = useCallback(() => setReloadKey((key) => key + 1), []);

  return { state, reload, searching: name.trim() !== trimmed };
}

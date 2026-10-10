import { useCallback, useEffect, useRef, useState } from 'react';
import { useLocalityRepository } from '@/hooks/useLocalityRepository';
import { getApiErrorMessage } from '@/lib/apiError';
import type { Locality } from '@/modules/localities/domain/Locality';

type State =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'ready'; localities: Locality[] }
  | { status: 'error'; message: string };

/**
 * All the localities of one province (ordered by name; the API does not paginate). Pass `undefined` to skip the
 * request. `generation` keeps a late response or error of a previous province from reaching the current one.
 */
export function useProvinceLocalities(provinceId: string | undefined) {
  const repository = useLocalityRepository();
  const [state, setState] = useState<State>({ status: 'idle' });
  const [reloadKey, setReloadKey] = useState(0);
  const generation = useRef(0);

  useEffect(() => {
    const mine = ++generation.current;
    if (!provinceId) {
      setState({ status: 'idle' });
      return;
    }
    setState({ status: 'loading' });
    repository
      .listByProvince(provinceId)
      .then((localities) => {
        if (generation.current === mine) setState({ status: 'ready', localities });
      })
      .catch((error: unknown) => {
        if (generation.current === mine) setState({ status: 'error', message: getApiErrorMessage(error) });
      });
    return () => {
      generation.current += 1;
    };
  }, [provinceId, repository, reloadKey]);

  const reload = useCallback(() => setReloadKey((key) => key + 1), []);

  return { state, reload };
}

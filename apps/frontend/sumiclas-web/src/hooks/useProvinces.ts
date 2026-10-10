import { useCallback, useEffect, useRef, useState } from 'react';
import { useProvinceRepository } from '@/hooks/useProvinceRepository';
import { getApiErrorMessage } from '@/lib/apiError';
import type { Province } from '@/modules/provinces/domain/Province';

type State =
  | { status: 'loading' }
  | { status: 'ready'; provinces: Province[] }
  | { status: 'error'; message: string };

export function useProvinces() {
  const repository = useProvinceRepository();
  const [state, setState] = useState<State>({ status: 'loading' });
  const [reloadKey, setReloadKey] = useState(0);
  const generation = useRef(0);

  useEffect(() => {
    const mine = ++generation.current;
    setState({ status: 'loading' });
    repository
      .listAll()
      .then((provinces) => {
        if (generation.current === mine) setState({ status: 'ready', provinces });
      })
      .catch((error: unknown) => {
        if (generation.current === mine) setState({ status: 'error', message: getApiErrorMessage(error) });
      });
    return () => {
      generation.current += 1;
    };
  }, [repository, reloadKey]);

  const reload = useCallback(() => setReloadKey((key) => key + 1), []);

  return { state, reload };
}

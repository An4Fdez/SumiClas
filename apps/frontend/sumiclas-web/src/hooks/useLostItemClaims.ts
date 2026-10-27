import { useCallback, useEffect, useRef, useState } from 'react';
import { useLostItemRepository } from '@/hooks/useLostItemRepository';
import { getApiErrorMessage } from '@/lib/apiError';
import type { LostItemClaim } from '@/modules/lostItems/domain/LostItemClaim';

type State =
  | { status: 'loading' }
  | { status: 'ready'; claims: LostItemClaim[] }
  | { status: 'error'; message: string };

/**
 * The claims of one lost item that the current user may see (all of them for the author, only their own for
 * anyone else). `generation` discards a late response or error of a previous item or reload.
 */
export function useLostItemClaims(lostItemId: string) {
  const repository = useLostItemRepository();
  const [state, setState] = useState<State>({ status: 'loading' });
  const [reloadKey, setReloadKey] = useState(0);
  const generation = useRef(0);

  useEffect(() => {
    const mine = ++generation.current;
    repository
      .listClaims(lostItemId)
      .then((claims) => {
        if (generation.current === mine) setState({ status: 'ready', claims });
      })
      .catch((error: unknown) => {
        if (generation.current === mine) setState({ status: 'error', message: getApiErrorMessage(error) });
      });
    return () => {
      generation.current += 1;
    };
  }, [lostItemId, repository, reloadKey]);

  const reload = useCallback(() => setReloadKey((key) => key + 1), []);

  return { state, reload };
}

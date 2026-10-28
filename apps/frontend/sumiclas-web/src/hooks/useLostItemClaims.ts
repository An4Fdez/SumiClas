import { useCallback, useEffect, useRef, useState } from 'react';
import { useLostItemRepository } from '@/hooks/useLostItemRepository';
import { useWizardIds } from '@/hooks/useWizardIds';
import { getApiErrorMessage, getApiErrorStatus, UserFacingError } from '@/lib/apiError';
import type { LostItem } from '@/modules/lostItems/domain/LostItem';
import type { LostItemClaim } from '@/modules/lostItems/domain/LostItemClaim';

const TRANSIENT_CLIENT_ERRORS = [408, 429];
const CONFLICT_STATUS = 409;

type State =
  | { status: 'loading' }
  | { status: 'ready'; claims: LostItemClaim[] }
  | { status: 'error'; message: string };

/**
 * The claims of one lost item that the current user may see (all of them for the author, only their own for
 * anyone else) and the actions on them. `generation` discards a late response or error of a previous item or reload.
 * `onChanged` receives the patch the item needs after each action (the lists show claim counts and status).
 */
export function useLostItemClaims(item: LostItem, onChanged: (patch: Partial<LostItem>) => void) {
  const lostItemId = item.id;
  const repository = useLostItemRepository();
  const ids = useWizardIds();
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

  const createClaim = async (message: string) => {
    const key = `lostItemClaim:${lostItemId}:${message}`;
    let id: string;
    try {
      id = ids.take(key);
    } catch {
      throw new UserFacingError('Tu navegador no permite enviar reclamaciones fuera de una conexión segura (HTTPS).');
    }
    try {
      await repository.createClaim(lostItemId, { id, message });
    } catch (error) {
      const status = getApiErrorStatus(error);
      if (status !== undefined && status < 500 && !TRANSIENT_CLIENT_ERRORS.includes(status)) ids.consume(key);
      if (status === CONFLICT_STATUS) {
        onChanged({ myClaimStatus: 'pending' });
        reload();
      }
      throw error;
    }
    ids.consume(key);
    onChanged({ myClaimStatus: 'pending' });
    reload();
  };

  const acceptClaim = async (claimId: string, responseMessage: string) => {
    await repository.acceptClaim(lostItemId, claimId, responseMessage || undefined);
    onChanged({ recovered: true, pendingClaimsCount: 0 });
    reload();
  };

  const rejectClaim = async (claimId: string) => {
    await repository.rejectClaim(lostItemId, claimId);
    onChanged({ pendingClaimsCount: Math.max((item.pendingClaimsCount ?? 1) - 1, 0) });
    reload();
  };

  return { state, reload, createClaim, acceptClaim, rejectClaim };
}

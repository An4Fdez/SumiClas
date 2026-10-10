import { useCallback, useEffect, useRef, useState } from 'react';
import { useChildContext } from '@/hooks/useChildContext';
import { useClassMembers } from '@/hooks/useClassMembers';
import { useUserRepository } from '@/hooks/useUserRepository';
import { getApiErrorMessage, getApiErrorStatus } from '@/lib/apiError';

export type RepresentativeAction =
  | { kind: 'assign'; userId: string }
  | { kind: 'transfer'; userId: string }
  | { kind: 'resign' };

const PARTIAL_TRANSFER_MESSAGE =
  'La otra persona ya es delegado/a, pero no hemos podido completar tu renuncia: sigues siendo delegado/a. Puedes reintentar la renuncia.';

function outcomeIsUnknown(error: unknown): boolean {
  const status = getApiErrorStatus(error);
  return status === undefined || status >= 500;
}

/**
 * Members of a class plus the delegate actions of the current user. There is no atomic transfer in the API:
 * transfer = assign the other person, then resign. `generation` drops the outcome of a request started for a
 * previous class, and `inFlight` ignores a second click while one request is running.
 */
export function useClassRepresentatives(classId: string | undefined) {
  const repository = useUserRepository();
  const { refresh, currentRole } = useChildContext();
  const { state, reload } = useClassMembers(classId);
  const [busy, setBusy] = useState<RepresentativeAction | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [resignPending, setResignPending] = useState(false);
  const inFlight = useRef(false);
  const generation = useRef(0);

  useEffect(() => {
    generation.current += 1;
    inFlight.current = false;
    setBusy(null);
    setActionError(null);
    setResignPending(false);
    return () => {
      generation.current += 1;
    };
  }, [classId]);

  useEffect(() => {
    if (resignPending && currentRole !== 'classRepresentative') {
      setResignPending(false);
      setActionError(null);
    }
  }, [resignPending, currentRole]);

  const settle = useCallback(async () => {
    reload();
    await refresh();
  }, [reload, refresh]);

  const run = useCallback(
    async (action: RepresentativeAction, work: (mine: number, target: string) => Promise<void>) => {
      if (!classId || inFlight.current) return;
      const mine = generation.current;
      inFlight.current = true;
      setBusy(action);
      setActionError(null);
      try {
        await work(mine, classId);
      } finally {
        if (generation.current === mine) {
          inFlight.current = false;
          setBusy(null);
        }
      }
    },
    [classId],
  );

  const fail = useCallback(
    (mine: number, error: unknown) => {
      if (generation.current !== mine) return;
      setActionError(getApiErrorMessage(error));
      if (outcomeIsUnknown(error)) reload();
    },
    [reload],
  );

  const assign = useCallback(
    (userId: string) =>
      run({ kind: 'assign', userId }, async (mine, target) => {
        try {
          await repository.assignClassRepresentative(userId, target);
        } catch (error) {
          fail(mine, error);
          return;
        }
        await settle();
      }),
    [run, repository, fail, settle],
  );

  const transfer = useCallback(
    (userId: string) =>
      run({ kind: 'transfer', userId }, async (mine, target) => {
        try {
          await repository.assignClassRepresentative(userId, target);
        } catch (error) {
          fail(mine, error);
          return;
        }
        try {
          await repository.resignClassRepresentative(target);
        } catch {
          if (generation.current === mine) {
            setResignPending(true);
            setActionError(PARTIAL_TRANSFER_MESSAGE);
          }
          await settle();
          return;
        }
        await settle();
      }),
    [run, repository, fail, settle],
  );

  const resign = useCallback(
    () =>
      run({ kind: 'resign' }, async (mine, target) => {
        try {
          await repository.resignClassRepresentative(target);
        } catch (error) {
          fail(mine, error);
          return;
        }
        if (generation.current === mine) setResignPending(false);
        await settle();
      }),
    [run, repository, fail, settle],
  );

  const dismissError = useCallback(() => {
    setActionError(null);
    setResignPending(false);
  }, []);

  return { state, reload, busy, actionError, resignPending, assign, transfer, resign, dismissError };
}

import { useCallback, useEffect, useRef, useState } from 'react';
import { useUserRepository } from '@/hooks/useUserRepository';
import { getApiErrorMessage } from '@/lib/apiError';
import type { ClassMember } from '@/modules/users/domain/ClassMember';

type State =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'ready'; members: ClassMember[] }
  | { status: 'error'; message: string };

/**
 * The members of one class (delegates first, then by name). Only a member of the class can read the list, so a
 * 403 surfaces as an error state. Pass `undefined` to skip the request. `generation` keeps a late response or error
 * of a previous class from reaching the current one.
 */
export function useClassMembers(classId: string | undefined) {
  const repository = useUserRepository();
  const [state, setState] = useState<State>({ status: 'idle' });
  const [reloadKey, setReloadKey] = useState(0);
  const generation = useRef(0);

  useEffect(() => {
    const mine = ++generation.current;
    if (!classId) {
      setState({ status: 'idle' });
      return;
    }
    setState({ status: 'loading' });
    repository
      .listClassMembers(classId)
      .then((members) => {
        if (generation.current === mine) setState({ status: 'ready', members });
      })
      .catch((error: unknown) => {
        if (generation.current === mine) setState({ status: 'error', message: getApiErrorMessage(error) });
      });
    return () => {
      generation.current += 1;
    };
  }, [classId, repository, reloadKey]);

  const reload = useCallback(() => setReloadKey((key) => key + 1), []);

  return { state, reload };
}

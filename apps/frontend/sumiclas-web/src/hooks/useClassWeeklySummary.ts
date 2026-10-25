import { useCallback, useEffect, useRef, useState } from 'react';
import { useWeeklySummaryRepository } from '@/hooks/useWeeklySummaryRepository';
import { getApiErrorMessage, getApiErrorStatus } from '@/lib/apiError';
import type { WeeklySummary } from '@/modules/weeklySummary/domain/WeeklySummary';

export type WeeklySummaryState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'ready'; summary: WeeklySummary }
  | { status: 'error'; message: string };

const CONFLICT_STATUS = 409;

/**
 * The summary of the current week of one class, asked for when the class is known and again whenever `generate` is
 * called. Several families opening the dashboard at the same time can race to store the same summary: the one that
 * loses gets a conflict, and asking once more finds the stored one. `generation` changes whenever the class or the
 * request changes, so a late answer of a previous request never reaches the current one.
 */
export function useClassWeeklySummary(classId: string | undefined) {
  const repository = useWeeklySummaryRepository();
  const [state, setState] = useState<WeeklySummaryState>({ status: 'idle' });
  const [requestKey, setRequestKey] = useState(0);
  const generation = useRef(0);

  useEffect(() => {
    const mine = ++generation.current;
    if (!classId) {
      setState({ status: 'idle' });
      return;
    }
    setState({ status: 'loading' });
    repository
      .generate(classId)
      .catch((error: unknown) => {
        if (getApiErrorStatus(error) !== CONFLICT_STATUS || generation.current !== mine) throw error;
        return repository.generate(classId);
      })
      .then((summary) => {
        if (generation.current === mine) setState({ status: 'ready', summary });
      })
      .catch((error: unknown) => {
        if (generation.current === mine) setState({ status: 'error', message: getApiErrorMessage(error) });
      });
    return () => {
      generation.current += 1;
    };
  }, [classId, repository, requestKey]);

  const generate = useCallback(() => setRequestKey((key) => key + 1), []);

  return { state, generate };
}

import { useCallback, useEffect, useRef, useState } from 'react';
import { useDashboardRepository } from '@/hooks/useDashboardRepository';
import { getApiErrorMessage } from '@/lib/apiError';
import type { DashboardSummary } from '@/modules/dashboard/domain/DashboardSummary';

type State =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'ready'; summary: DashboardSummary }
  | { status: 'error'; message: string };

/**
 * The dashboard numbers of one class over a period of days. `generation` changes whenever the class, the period
 * or the load changes, so a late response or a late error of a previous load never reaches the current one.
 */
export function useClassDashboardSummary(classId: string | undefined, days: number) {
  const repository = useDashboardRepository();
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
      .getSummary(classId, days)
      .then((summary) => {
        if (generation.current === mine) setState({ status: 'ready', summary });
      })
      .catch((error: unknown) => {
        if (generation.current === mine) setState({ status: 'error', message: getApiErrorMessage(error) });
      });
    return () => {
      generation.current += 1;
    };
  }, [classId, days, repository, reloadKey]);

  const reload = useCallback(() => setReloadKey((key) => key + 1), []);

  return { state, reload };
}

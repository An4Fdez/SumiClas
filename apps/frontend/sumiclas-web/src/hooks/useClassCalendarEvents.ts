import { useCallback, useEffect, useRef, useState } from 'react';
import { useCalendarEventRepository } from '@/hooks/useCalendarEventRepository';
import { getApiErrorMessage } from '@/lib/apiError';
import type { CalendarEvent } from '@/modules/calendarEvents/domain/CalendarEvent';

type State =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'ready'; events: CalendarEvent[] }
  | { status: 'error'; message: string };

/**
 * Every event of one class: the school-wide ones, the class's one-off events and the birthdays of its students.
 * `generation` changes whenever the class or the list is reloaded, so a late response or a late error of a
 * previous load never reaches the current one.
 */
export function useClassCalendarEvents(classId: string | undefined) {
  const repository = useCalendarEventRepository();
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
      .listByClass(classId)
      .then((events) => {
        if (generation.current === mine) setState({ status: 'ready', events });
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

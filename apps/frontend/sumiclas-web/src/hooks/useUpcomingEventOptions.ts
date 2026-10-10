import { useMemo } from 'react';
import { useClassCalendarEvents } from '@/hooks/useClassCalendarEvents';
import { nextOccurrence } from '@/lib/date';

/** The upcoming one-off events of a class (never birthdays) as `{id, title}` options. Pass `undefined` to skip the request. */
export function useUpcomingEventOptions(classId: string | undefined) {
  const { state } = useClassCalendarEvents(classId);
  return useMemo(() => {
    if (state.status !== 'ready') return undefined;
    const today = new Date();
    return state.events
      .filter((event) => event.type !== 'birthday' && nextOccurrence(event, today) !== undefined)
      .map((event) => ({ id: event.id, title: event.title }));
  }, [state]);
}

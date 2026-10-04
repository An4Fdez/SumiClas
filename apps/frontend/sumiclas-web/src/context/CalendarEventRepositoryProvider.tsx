import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagCalendarEventRepository } from '@/modules/calendarEvents/infrastructure/NSwagCalendarEventRepository';
import { CalendarEventRepositoryContext } from './CalendarEventRepositoryContext';

export function CalendarEventRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new NSwagCalendarEventRepository(client), [client]);

  return <CalendarEventRepositoryContext.Provider value={repository}>{children}</CalendarEventRepositoryContext.Provider>;
}

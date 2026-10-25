import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagWeeklySummaryRepository } from '@/modules/weeklySummary/infrastructure/NSwagWeeklySummaryRepository';
import { WeeklySummaryRepositoryContext } from './WeeklySummaryRepositoryContext';

export function WeeklySummaryRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new NSwagWeeklySummaryRepository(client), [client]);

  return <WeeklySummaryRepositoryContext.Provider value={repository}>{children}</WeeklySummaryRepositoryContext.Provider>;
}

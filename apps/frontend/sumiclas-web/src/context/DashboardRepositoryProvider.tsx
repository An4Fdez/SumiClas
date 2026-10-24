import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagDashboardRepository } from '@/modules/dashboard/infrastructure/NSwagDashboardRepository';
import { DashboardRepositoryContext } from './DashboardRepositoryContext';

export function DashboardRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new NSwagDashboardRepository(client), [client]);

  return <DashboardRepositoryContext.Provider value={repository}>{children}</DashboardRepositoryContext.Provider>;
}

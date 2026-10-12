import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagSessionRepository } from '@/modules/sessions/infrastructure/NSwagSessionRepository';
import { SessionRepositoryContext } from './SessionRepositoryContext';

export function SessionRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new NSwagSessionRepository(client), [client]);

  return <SessionRepositoryContext.Provider value={repository}>{children}</SessionRepositoryContext.Provider>;
}

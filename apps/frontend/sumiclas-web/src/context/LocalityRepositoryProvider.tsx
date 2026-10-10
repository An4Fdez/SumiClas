import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagLocalityRepository } from '@/modules/localities/infrastructure/NSwagLocalityRepository';
import { LocalityRepositoryContext } from './LocalityRepositoryContext';

export function LocalityRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new NSwagLocalityRepository(client), [client]);

  return <LocalityRepositoryContext.Provider value={repository}>{children}</LocalityRepositoryContext.Provider>;
}

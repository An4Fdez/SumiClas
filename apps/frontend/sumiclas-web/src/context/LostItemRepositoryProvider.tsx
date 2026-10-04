import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagLostItemRepository } from '@/modules/lostItems/infrastructure/NSwagLostItemRepository';
import { LostItemRepositoryContext } from './LostItemRepositoryContext';

export function LostItemRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new NSwagLostItemRepository(client), [client]);

  return <LostItemRepositoryContext.Provider value={repository}>{children}</LostItemRepositoryContext.Provider>;
}

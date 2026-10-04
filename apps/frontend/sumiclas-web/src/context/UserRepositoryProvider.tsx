import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagUserRepository } from '@/modules/users/infrastructure/NSwagUserRepository';
import { UserRepositoryContext } from './UserRepositoryContext';

export function UserRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new NSwagUserRepository(client), [client]);

  return <UserRepositoryContext.Provider value={repository}>{children}</UserRepositoryContext.Provider>;
}

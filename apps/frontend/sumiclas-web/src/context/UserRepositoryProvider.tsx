import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { UserRepositoryHttp } from '@/modules/users/infrastructure/UserRepositoryHttp';
import { UserRepositoryContext } from './UserRepositoryContext';

export function UserRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new UserRepositoryHttp(client), [client]);

  return <UserRepositoryContext.Provider value={repository}>{children}</UserRepositoryContext.Provider>;
}

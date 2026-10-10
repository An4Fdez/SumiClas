import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagAuthenticationRepository } from '@/modules/authentication/infrastructure/NSwagAuthenticationRepository';
import { AuthenticationRepositoryContext } from './AuthenticationRepositoryContext';

export function AuthenticationRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new NSwagAuthenticationRepository(client), [client]);

  return <AuthenticationRepositoryContext.Provider value={repository}>{children}</AuthenticationRepositoryContext.Provider>;
}

import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagProvinceRepository } from '@/modules/provinces/infrastructure/NSwagProvinceRepository';
import { ProvinceRepositoryContext } from './ProvinceRepositoryContext';

export function ProvinceRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new NSwagProvinceRepository(client), [client]);

  return <ProvinceRepositoryContext.Provider value={repository}>{children}</ProvinceRepositoryContext.Provider>;
}

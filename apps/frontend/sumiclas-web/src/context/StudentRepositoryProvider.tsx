import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { StudentRepositoryHttp } from '@/modules/students/infrastructure/StudentRepositoryHttp';
import { StudentRepositoryContext } from './StudentRepositoryContext';

export function StudentRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new StudentRepositoryHttp(client), [client]);

  return <StudentRepositoryContext.Provider value={repository}>{children}</StudentRepositoryContext.Provider>;
}

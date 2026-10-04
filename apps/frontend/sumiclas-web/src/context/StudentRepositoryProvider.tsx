import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagStudentRepository } from '@/modules/students/infrastructure/NSwagStudentRepository';
import { StudentRepositoryContext } from './StudentRepositoryContext';

export function StudentRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new NSwagStudentRepository(client), [client]);

  return <StudentRepositoryContext.Provider value={repository}>{children}</StudentRepositoryContext.Provider>;
}

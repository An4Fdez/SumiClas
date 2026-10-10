import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagClassExpenseRepository } from '@/modules/classExpenses/infrastructure/NSwagClassExpenseRepository';
import { ClassExpenseRepositoryContext } from './ClassExpenseRepositoryContext';

export function ClassExpenseRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new NSwagClassExpenseRepository(client), [client]);

  return <ClassExpenseRepositoryContext.Provider value={repository}>{children}</ClassExpenseRepositoryContext.Provider>;
}

import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagAssignmentRepository } from '@/modules/assignments/infrastructure/NSwagAssignmentRepository';
import { NSwagSubjectRepository } from '@/modules/subjects/infrastructure/NSwagSubjectRepository';
import { HomeworkRepositoriesContext, type HomeworkRepositories } from './HomeworkRepositoriesContext';

export function HomeworkRepositoriesProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repositories = useMemo<HomeworkRepositories>(
    () => ({
      assignments: new NSwagAssignmentRepository(client),
      subjects: new NSwagSubjectRepository(client),
    }),
    [client],
  );

  return <HomeworkRepositoriesContext.Provider value={repositories}>{children}</HomeworkRepositoriesContext.Provider>;
}

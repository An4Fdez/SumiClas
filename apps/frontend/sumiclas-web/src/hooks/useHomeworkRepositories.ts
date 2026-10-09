import { useContext } from 'react';
import { HomeworkRepositoriesContext } from '@/context/HomeworkRepositoriesContext';

export function useHomeworkRepositories() {
  const repositories = useContext(HomeworkRepositoriesContext);
  if (!repositories) throw new Error('useHomeworkRepositories must be used within HomeworkRepositoriesProvider');
  return repositories;
}

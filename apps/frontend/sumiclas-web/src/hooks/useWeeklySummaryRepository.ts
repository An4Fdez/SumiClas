import { useContext } from 'react';
import { WeeklySummaryRepositoryContext } from '@/context/WeeklySummaryRepositoryContext';

export function useWeeklySummaryRepository() {
  const repository = useContext(WeeklySummaryRepositoryContext);
  if (!repository) throw new Error('useWeeklySummaryRepository must be used within WeeklySummaryRepositoryProvider');
  return repository;
}

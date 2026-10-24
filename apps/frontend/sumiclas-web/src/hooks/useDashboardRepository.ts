import { useContext } from 'react';
import { DashboardRepositoryContext } from '@/context/DashboardRepositoryContext';

export function useDashboardRepository() {
  const repository = useContext(DashboardRepositoryContext);
  if (!repository) throw new Error('useDashboardRepository must be used within DashboardRepositoryProvider');
  return repository;
}

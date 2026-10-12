import { useContext } from 'react';
import { SessionRepositoryContext } from '@/context/SessionRepositoryContext';

export function useSessionRepository() {
  const repository = useContext(SessionRepositoryContext);
  if (!repository) throw new Error('useSessionRepository must be used within SessionRepositoryProvider');
  return repository;
}

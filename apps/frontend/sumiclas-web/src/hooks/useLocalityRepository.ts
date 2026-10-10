import { useContext } from 'react';
import { LocalityRepositoryContext } from '@/context/LocalityRepositoryContext';

export function useLocalityRepository() {
  const repository = useContext(LocalityRepositoryContext);
  if (!repository) throw new Error('useLocalityRepository must be used within LocalityRepositoryProvider');
  return repository;
}

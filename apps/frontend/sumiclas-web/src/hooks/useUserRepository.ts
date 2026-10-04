import { useContext } from 'react';
import { UserRepositoryContext } from '@/context/UserRepositoryContext';

export function useUserRepository() {
  const repository = useContext(UserRepositoryContext);
  if (!repository) throw new Error('useUserRepository must be used within UserRepositoryProvider');
  return repository;
}

import { useContext } from 'react';
import { AuthenticationRepositoryContext } from '@/context/AuthenticationRepositoryContext';

export function useAuthenticationRepository() {
  const repository = useContext(AuthenticationRepositoryContext);
  if (!repository) throw new Error('useAuthenticationRepository must be used within AuthenticationRepositoryProvider');
  return repository;
}

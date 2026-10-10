import { useContext } from 'react';
import { ClassExpenseRepositoryContext } from '@/context/ClassExpenseRepositoryContext';

export function useClassExpenseRepository() {
  const repository = useContext(ClassExpenseRepositoryContext);
  if (!repository) throw new Error('useClassExpenseRepository must be used within ClassExpenseRepositoryProvider');
  return repository;
}

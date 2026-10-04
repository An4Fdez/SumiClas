import { useContext } from 'react';
import { StudentRepositoryContext } from '@/context/StudentRepositoryContext';

export function useStudentRepository() {
  const repository = useContext(StudentRepositoryContext);
  if (!repository) throw new Error('useStudentRepository must be used within StudentRepositoryProvider');
  return repository;
}

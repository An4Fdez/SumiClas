import { useContext } from 'react';
import { ProvinceRepositoryContext } from '@/context/ProvinceRepositoryContext';

export function useProvinceRepository() {
  const repository = useContext(ProvinceRepositoryContext);
  if (!repository) throw new Error('useProvinceRepository must be used within ProvinceRepositoryProvider');
  return repository;
}

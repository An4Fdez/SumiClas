import { useContext } from 'react';
import { LostItemRepositoryContext } from '@/context/LostItemRepositoryContext';

export function useLostItemRepository() {
  const repository = useContext(LostItemRepositoryContext);
  if (!repository) throw new Error('useLostItemRepository must be used within LostItemRepositoryProvider');
  return repository;
}

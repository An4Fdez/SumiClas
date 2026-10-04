import { useContext } from 'react';
import { AnnouncementRepositoryContext } from '@/context/AnnouncementRepositoryContext';

export function useAnnouncementRepository() {
  const repository = useContext(AnnouncementRepositoryContext);
  if (!repository) throw new Error('useAnnouncementRepository must be used within AnnouncementRepositoryProvider');
  return repository;
}

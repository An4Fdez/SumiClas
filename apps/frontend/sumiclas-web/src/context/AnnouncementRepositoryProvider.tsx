import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagAnnouncementRepository } from '@/modules/announcements/infrastructure/NSwagAnnouncementRepository';
import { AnnouncementRepositoryContext } from './AnnouncementRepositoryContext';

export function AnnouncementRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new NSwagAnnouncementRepository(client), [client]);

  return <AnnouncementRepositoryContext.Provider value={repository}>{children}</AnnouncementRepositoryContext.Provider>;
}

import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { AnnouncementRepositoryHttp } from '@/modules/announcements/infrastructure/AnnouncementRepositoryHttp';
import { AnnouncementRepositoryContext } from './AnnouncementRepositoryContext';

export function AnnouncementRepositoryProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repository = useMemo(() => new AnnouncementRepositoryHttp(client), [client]);

  return <AnnouncementRepositoryContext.Provider value={repository}>{children}</AnnouncementRepositoryContext.Provider>;
}

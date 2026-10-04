import { createContext } from 'react';
import type { IAnnouncementRepository } from '@/modules/announcements/domain/IAnnouncementRepository';

export const AnnouncementRepositoryContext = createContext<IAnnouncementRepository | null>(null);

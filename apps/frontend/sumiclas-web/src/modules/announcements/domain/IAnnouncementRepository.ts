import type { PageRequest } from '@/modules/Shared/domain/PageRequest';
import type { AnnouncementChanges } from './AnnouncementChanges';
import type { AnnouncementPage } from './AnnouncementPage';
import type { NewAnnouncement } from './NewAnnouncement';

export interface IAnnouncementRepository {
  listByClass(classId: string, page?: PageRequest): Promise<AnnouncementPage>;
  create(announcement: NewAnnouncement): Promise<void>;
  update(id: string, changes: AnnouncementChanges): Promise<void>;
  remove(id: string): Promise<void>;
  markAsRead(id: string): Promise<void>;
}

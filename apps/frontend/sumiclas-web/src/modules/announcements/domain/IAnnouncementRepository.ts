import type { PageRequest } from '@/modules/Shared/domain/PageRequest';
import type { AnnouncementPage } from './AnnouncementPage';
import type { NewAnnouncement } from './NewAnnouncement';

export interface IAnnouncementRepository {
  listByClass(classId: string, page?: PageRequest): Promise<AnnouncementPage>;
  create(announcement: NewAnnouncement): Promise<void>;
  markAsRead(id: string): Promise<void>;
}

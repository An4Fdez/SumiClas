import type { ClassRole } from '@/lib/roles';

export type Priority = 'low' | 'normal' | 'high';
export type AnnouncementScope = 'school' | 'class';

export type RelatedEvent = {
  id: string;
  title: string;
};

export type Announcement = {
  id: string;
  scope: AnnouncementScope;
  classId?: string;
  title: string;
  body: string;
  priority: Priority;
  authorId: string;
  authorName: string;
  authorRole?: ClassRole;
  createdAt: Date;
  relatedEvent?: RelatedEvent;
  read: boolean;
};

export type AnnouncementPage = {
  items: Announcement[];
  nextCursor?: string;
};

export type PageRequest = {
  limit?: number;
  cursor?: string;
};

export type NewAnnouncement = {
  id: string;
  scope: AnnouncementScope;
  classId?: string;
  title: string;
  body: string;
  priority: Priority;
  relatedEventId?: string;
};

export interface IAnnouncementRepository {
  listByClass(classId: string, page?: PageRequest): Promise<AnnouncementPage>;
  create(announcement: NewAnnouncement): Promise<void>;
  markAsRead(id: string): Promise<void>;
}

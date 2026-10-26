import type { ClassRole } from '@/lib/roles';
import type { AnnouncementScope } from './AnnouncementScope';
import type { Priority } from './Priority';
import type { RelatedEvent } from './RelatedEvent';

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
  updatedAt?: Date;
  relatedEvent?: RelatedEvent;
  read: boolean;
  isAuthor: boolean;
};

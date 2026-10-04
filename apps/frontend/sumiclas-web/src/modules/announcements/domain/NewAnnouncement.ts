import type { AnnouncementScope } from './AnnouncementScope';
import type { Priority } from './Priority';

export type NewAnnouncement = {
  id: string;
  scope: AnnouncementScope;
  classId?: string;
  title: string;
  body: string;
  priority: Priority;
  relatedEventId?: string;
};

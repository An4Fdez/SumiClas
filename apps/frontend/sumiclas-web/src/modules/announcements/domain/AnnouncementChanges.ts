import type { Priority } from './Priority';

export type AnnouncementChanges = {
  title: string;
  body: string;
  priority: Priority;
  relatedEventId?: string;
};

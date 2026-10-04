import type { Announcement } from './Announcement';

export type AnnouncementPage = {
  items: Announcement[];
  nextCursor?: string;
};

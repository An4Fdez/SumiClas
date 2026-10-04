import type { OneOffEventType } from './OneOffEventType';

export type NewCalendarEvent = {
  id: string;
  classId: string;
  type: OneOffEventType;
  title: string;
  date: Date;
  time?: string;
  location?: string;
  cost?: string;
};

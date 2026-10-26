import type { OneOffEventType } from './OneOffEventType';

export type CalendarEventChanges = {
  type: OneOffEventType;
  title: string;
  date: Date;
  time?: string;
  location?: string;
  cost?: string;
};

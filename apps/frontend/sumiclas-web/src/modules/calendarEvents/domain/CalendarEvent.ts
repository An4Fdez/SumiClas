import type { ClassRole } from '@/lib/roles';
import type { EventScope } from './EventScope';
import type { EventType } from './EventType';

/** `year` is undefined for birthdays: they recur every year and a birth year is never stored. */
export type CalendarEvent = {
  id: string;
  scope: EventScope;
  classId?: string;
  studentId?: string;
  type: EventType;
  title: string;
  month: number;
  day: number;
  year?: number;
  time?: string;
  location?: string;
  cost?: string;
  authorId: string;
  authorName: string;
  authorRole?: ClassRole;
  createdAt: Date;
  updatedAt?: Date;
  isAuthor: boolean;
};

import type { CalendarEvent } from './CalendarEvent';
import type { NewBirthday } from './NewBirthday';
import type { NewCalendarEvent } from './NewCalendarEvent';

export interface ICalendarEventRepository {
  listByClass(classId: string): Promise<CalendarEvent[]>;
  create(event: NewCalendarEvent): Promise<void>;
  createBirthday(birthday: NewBirthday): Promise<void>;
}

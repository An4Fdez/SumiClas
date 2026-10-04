import type { ClassRole } from '@/lib/roles';
import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import { toApiDateOnly } from '@/modules/Shared/Infrastructure/apiDate';
import type { CalendarEvent } from '../domain/CalendarEvent';
import type { EventType } from '../domain/EventType';
import type { ICalendarEventRepository } from '../domain/ICalendarEventRepository';
import type { NewBirthday } from '../domain/NewBirthday';
import type { NewCalendarEvent } from '../domain/NewCalendarEvent';

const EVENT_TYPES: EventType[] = ['trip', 'meeting', 'activity', 'birthday', 'other'];

function toType(value: string): EventType {
  const type = value.toLowerCase() as EventType;
  return EVENT_TYPES.includes(type) ? type : 'other';
}

function toRole(value: string | undefined): ClassRole | undefined {
  if (value === undefined || value === null) return undefined;
  return value.toLowerCase() === 'classrepresentative' ? 'classRepresentative' : 'participant';
}

function toTime(value: string | undefined): string | undefined {
  if (value === undefined || value === null) return undefined;
  return value.slice(0, 5);
}

function capitalize(value: string): string {
  return value.charAt(0).toUpperCase() + value.slice(1);
}

export class NSwagCalendarEventRepository implements ICalendarEventRepository {
  constructor(private readonly client: ApiClient) {}

  async listByClass(classId: string): Promise<CalendarEvent[]> {
    const events = await this.client.calendarEventFinderByClass(classId);
    return (events ?? []).map((event) => ({
      id: event.id,
      scope: event.scope.toLowerCase() === 'school' ? 'school' : 'class',
      classId: event.classId ?? undefined,
      studentId: event.studentId ?? undefined,
      type: toType(event.type),
      title: event.title,
      month: event.month,
      day: event.day,
      year: event.year ?? undefined,
      time: toTime(event.time),
      location: event.location ?? undefined,
      cost: event.cost ?? undefined,
      authorId: event.authorId,
      authorName: event.authorName,
      authorRole: toRole(event.authorRole),
      createdAt: event.createdAt,
    }));
  }

  async create(event: NewCalendarEvent): Promise<void> {
    await this.client.calendarEventCreate({
      id: event.id,
      scope: 'Class',
      classId: event.classId,
      type: capitalize(event.type),
      title: event.title,
      date: toApiDateOnly(event.date),
      time: event.time,
      location: event.location,
      cost: event.cost,
    });
  }

  async createBirthday(birthday: NewBirthday): Promise<void> {
    await this.client.calendarEventBirthdayCreate({
      id: birthday.id,
      studentId: birthday.studentId,
      title: birthday.title,
      month: birthday.month,
      day: birthday.day,
    });
  }
}

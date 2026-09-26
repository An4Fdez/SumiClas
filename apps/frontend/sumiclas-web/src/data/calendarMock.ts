import { Bus, Cake, MoreHorizontal, Star, Users, type LucideIcon } from 'lucide-react';
import type { PaletteColor } from '@/lib/colorClasses';

export type EventType = 'trip' | 'meeting' | 'activity' | 'birthday' | 'other';
export type EventScope = 'school' | 'class';

export type CalendarEvent = {
  id: string;
  scope: EventScope;
  /** Only if scope === 'class': which child/class it belongs to. */
  childId?: string;
  type: EventType;
  title: string;
  /**
   * ISO. For type 'birthday' a sentinel year (1900) is used: it's structurally
   * impossible to derive the child's real age. Matching against the calendar
   * grid for these events is done by month+day only (see lib/date.ts).
   */
  date: string;
  time?: string;
  location?: string;
  /** Informational text only, never a payment status (that lives in Expenses). */
  cost?: string;
  authorName: string;
  authorRole: string;
};

export type RelatedEvent = Pick<CalendarEvent, 'id' | 'title'>;

export const eventTypeMeta: Record<EventType, { label: string; color: PaletteColor; icon: LucideIcon }> = {
  trip: { label: 'Excursión', color: 'mint', icon: Bus },
  meeting: { label: 'Reunión', color: 'violet', icon: Users },
  activity: { label: 'Actividad', color: 'sun', icon: Star },
  birthday: { label: 'Cumpleaños', color: 'rose', icon: Cake },
  other: { label: 'Otros', color: 'orange', icon: MoreHorizontal },
};

const now = new Date();
const inDays = (d: number, time?: { h: number; m: number }) => {
  const date = new Date(now);
  date.setDate(date.getDate() + d);
  if (time) date.setHours(time.h, time.m, 0, 0);
  return date.toISOString();
};

export const calendarEventsMock: CalendarEvent[] = [
  {
    id: 'ev1',
    scope: 'class',
    childId: 'h1',
    type: 'trip',
    title: 'Excursión al museo',
    date: inDays(3, { h: 9, m: 30 }),
    time: '09:30',
    location: 'Museo de la Ciudad',
    cost: '5 € por alumno',
    authorName: 'Elena Torres',
    authorRole: 'Profesora',
  },
  {
    id: 'ev2',
    scope: 'class',
    childId: 'h2',
    type: 'meeting',
    title: 'Reunión de padres',
    date: inDays(7, { h: 17, m: 0 }),
    time: '17:00',
    location: 'Aula 1ºB',
    authorName: 'Marta Ibáñez',
    authorRole: 'Delegada de padres',
  },
  {
    id: 'ev3',
    scope: 'school',
    type: 'activity',
    title: 'Mercadillo solidario del AMPA',
    date: inDays(10, { h: 10, m: 0 }),
    time: '10:00',
    location: 'Patio del colegio',
    authorName: 'AMPA',
    authorRole: 'AMPA',
  },
  {
    id: 'ev-cumple-1',
    scope: 'class',
    childId: 'h1',
    type: 'birthday',
    title: 'Cumpleaños de Martina',
    date: new Date(1900, 10, 5).toISOString(),
    authorName: 'Laura Sáez',
    authorRole: 'Familia',
  },
  {
    id: 'ev-cumple-2',
    scope: 'class',
    childId: 'h2',
    type: 'birthday',
    title: 'Cumpleaños de Pablo',
    date: new Date(1900, 2, 12).toISOString(),
    authorName: 'Marta Ibáñez',
    authorRole: 'Familia',
  },
];

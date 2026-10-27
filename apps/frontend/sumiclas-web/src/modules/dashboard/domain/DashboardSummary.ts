import type { EventType } from '@/modules/calendarEvents/domain/EventType';

export type UpcomingEvent = {
  title: string;
  type: EventType;
  date: Date;
  daysUntil: number;
};

export type DashboardSummary = {
  /** The period, in days, that the announcements, the lost items and the upcoming events cover. */
  days: number;
  announcements: { unread: number; urgent: number };
  homework: { pending: number; nextDueDate?: Date };
  expenses: { pending: boolean; item?: string };
  lostAndFound: { count: number };
  /** Pending claims on the user's own open lost items. */
  lostItemClaims: { count: number };
  /** Soonest first, so the first one is the next event of the class. */
  upcomingEvents: UpcomingEvent[];
};

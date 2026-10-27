import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { EventType } from '@/modules/calendarEvents/domain/EventType';
import type { DashboardSummary } from '../domain/DashboardSummary';
import type { IDashboardRepository } from '../domain/IDashboardRepository';

const EVENT_TYPES: EventType[] = ['trip', 'meeting', 'activity', 'birthday', 'other'];

function toType(value: string): EventType {
  const type = value.toLowerCase() as EventType;
  return EVENT_TYPES.includes(type) ? type : 'other';
}

export class NSwagDashboardRepository implements IDashboardRepository {
  constructor(private readonly client: ApiClient) {}

  async getSummary(classId: string, days: number): Promise<DashboardSummary> {
    const response = await this.client.dashboardSummaryFinderByClass(classId, days);
    return {
      days: response.days,
      announcements: { unread: response.announcements.unread, urgent: response.announcements.unreadUrgent },
      homework: {
        pending: response.assignments.pending,
        nextDueDate: response.assignments.nextDueDate ?? undefined,
      },
      expenses: { pending: response.expenses.pending, item: response.expenses.item ?? undefined },
      lostAndFound: { count: response.lostItems.count },
      lostItemClaims: { count: response.lostItemClaims.count },
      upcomingEvents: response.upcomingEvents.map((event) => ({
        title: event.title,
        type: toType(event.type),
        date: event.date,
        daysUntil: event.daysUntil,
      })),
    };
  }
}

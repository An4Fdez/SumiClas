import type { DashboardSummary } from './DashboardSummary';

export interface IDashboardRepository {
  getSummary(classId: string, days: number): Promise<DashboardSummary>;
}

import type { WeeklySummary } from './WeeklySummary';

export interface IWeeklySummaryRepository {
  /** The summary of the current week of the class; the server writes it only when the week changed since the last one. */
  generate(classId: string): Promise<WeeklySummary>;
}

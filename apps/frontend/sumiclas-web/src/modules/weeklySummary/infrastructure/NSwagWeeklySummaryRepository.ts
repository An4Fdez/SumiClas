import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { IWeeklySummaryRepository } from '../domain/IWeeklySummaryRepository';
import type { WeeklySummary } from '../domain/WeeklySummary';

const LINE_BREAK = /\r?\n/;

function toLines(content: string | null | undefined): string[] {
  return (content ?? '')
    .split(LINE_BREAK)
    .map((line) => line.trim())
    .filter((line) => line.length > 0);
}

export class NSwagWeeklySummaryRepository implements IWeeklySummaryRepository {
  constructor(private readonly client: ApiClient) {}

  async generate(classId: string): Promise<WeeklySummary> {
    const response = await this.client.weeklySummaryGenerate(classId);
    return {
      weekStart: response.weekStart,
      weekEnd: response.weekEnd,
      lines: toLines(response.content),
      generatedAt: response.generatedAt ?? undefined,
    };
  }
}

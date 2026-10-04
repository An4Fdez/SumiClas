import { getUserId } from '@/lib/auth';
import { GeneratedApiClient } from './GeneratedApiClient';

const API_BASE_URL = '/api';

const DATE_TIME_KEYS = new Set(['createdAt']);
const DATE_ONLY_KEYS = new Set(['dueDate', 'date', 'startDate', 'endDate']);
const ISO_DATE_TIME = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})$/;
const ISO_DATE_ONLY = /^(\d{4})-(\d{2})-(\d{2})$/;

/**
 * The generated DTOs type these fields as Date but JSON.parse leaves strings. Only the known date keys are
 * revived, so free text that happens to look like a date (a title, a description) is never converted.
 * Date-only values become LOCAL midnight so they never shift a day with the timezone.
 */
function reviveDates(key: string, value: unknown): unknown {
  if (typeof value !== 'string') return value;
  if (DATE_TIME_KEYS.has(key) && ISO_DATE_TIME.test(value)) return new Date(value);
  const dateOnly = DATE_ONLY_KEYS.has(key) ? ISO_DATE_ONLY.exec(value) : null;
  if (dateOnly) return new Date(Number(dateOnly[1]), Number(dateOnly[2]) - 1, Number(dateOnly[3]));
  return value;
}

/** Provisional identity: sends X-User-Id when a user id is stored. Read on every request, never cached. */
const identityHttp = {
  fetch(url: RequestInfo, init?: RequestInit): Promise<Response> {
    const userId = getUserId();
    if (!userId) return window.fetch(url, init);
    const headers = new Headers(init?.headers);
    headers.set('X-User-Id', userId);
    return window.fetch(url, { ...init, headers });
  },
};

export class ApiClient extends GeneratedApiClient {
  protected override jsonParseReviver = reviveDates;
}

export function createApiClient(): ApiClient {
  return new ApiClient(API_BASE_URL, identityHttp);
}

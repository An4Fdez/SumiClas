/**
 * Use for date-only request fields (dueDate, startDate, endDate, date). The DTOs type them as Date, but the API
 * expects "yyyy-MM-dd"; a plain Date would be serialized as an UTC date-time and rejected or shifted a day.
 * Wrap the value: `dueDate: toApiDateOnly(selectedDate)`.
 */
export function toApiDateOnly(date: Date): Date {
  const pad = (value: number) => String(value).padStart(2, '0');
  const text = `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
  return Object.assign(new Date(date.getTime()), { toJSON: () => text });
}

const DATE_TIME_KEYS = new Set(['createdAt', 'expiresAt']);
const DATE_ONLY_KEYS = new Set(['dueDate', 'nextDueDate', 'date', 'startDate', 'endDate']);
const ISO_DATE_TIME = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})$/;
const ISO_DATE_ONLY = /^(\d{4})-(\d{2})-(\d{2})$/;

/**
 * The generated DTOs type these fields as Date but JSON.parse leaves strings. Only the known date keys are
 * revived, so free text that happens to look like a date (a title, a description) is never converted.
 * Date-only values become LOCAL midnight so they never shift a day with the timezone.
 */
export function reviveApiDates(key: string, value: unknown): unknown {
  if (typeof value !== 'string') return value;
  if (DATE_TIME_KEYS.has(key) && ISO_DATE_TIME.test(value)) return new Date(value);
  const dateOnly = DATE_ONLY_KEYS.has(key) ? ISO_DATE_ONLY.exec(value) : null;
  if (dateOnly) return new Date(Number(dateOnly[1]), Number(dateOnly[2]) - 1, Number(dateOnly[3]));
  return value;
}

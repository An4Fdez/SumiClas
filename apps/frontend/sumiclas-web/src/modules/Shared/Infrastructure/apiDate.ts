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

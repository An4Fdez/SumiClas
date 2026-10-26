const MINUTES_PER_HOUR = 60;
const HOURS_PER_DAY = 24;
const DAYS_PER_WEEK = 7;
const MS_PER_MINUTE = 60_000;
const MS_PER_HOUR = MINUTES_PER_HOUR * MS_PER_MINUTE;
const MS_PER_DAY = HOURS_PER_DAY * MS_PER_HOUR;

const rtf = new Intl.RelativeTimeFormat('es', { numeric: 'auto' });

export function formatRelativeDate(dateISO: string | Date): string {
  const date = new Date(dateISO);
  const diffMs = date.getTime() - Date.now();
  const diffMin = Math.round(diffMs / MS_PER_MINUTE);
  const diffH = Math.round(diffMs / MS_PER_HOUR);
  const diffD = Math.round(diffMs / MS_PER_DAY);

  if (Math.abs(diffMin) < MINUTES_PER_HOUR) return rtf.format(diffMin, 'minute');
  if (Math.abs(diffH) < HOURS_PER_DAY) return rtf.format(diffH, 'hour');
  if (Math.abs(diffD) < DAYS_PER_WEEK) return rtf.format(diffD, 'day');

  return date.toLocaleDateString('es-ES', { day: 'numeric', month: 'short' });
}

export type CalendarDay = {
  date: Date;
  inCurrentMonth: boolean;
  isToday: boolean;
};

export function isSameCalendarDay(a: Date, b: Date): boolean {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

/** The date part of an event: month 1-12 and day; `year` is undefined for events that recur every year (birthdays). */
export type EventDay = { month: number; day: number; year?: number };

function isLeapYear(year: number): boolean {
  return (year % 4 === 0 && year % 100 !== 0) || year % 400 === 0;
}

/** The day an event takes place in a given year. A 29 February that recurs falls on 28 February in non-leap years. */
export function occurrenceInYear(event: EventDay, year: number): Date {
  const day = event.month === 2 && event.day === 29 && !isLeapYear(year) ? 28 : event.day;
  return new Date(year, event.month - 1, day);
}

/** Year to display an event in: its own year, or the current one for events that recur every year. */
export function visibleYearForEvent(event: EventDay): number {
  return event.year ?? new Date().getFullYear();
}

/** true if the event occurs on that day: once for a one-off event, every year for a recurring one. */
export function eventOccursOnDay(event: EventDay, day: Date): boolean {
  if (event.year !== undefined) return event.year === day.getFullYear() && isSameCalendarDay(occurrenceInYear(event, event.year), day);
  return isSameCalendarDay(occurrenceInYear(event, day.getFullYear()), day);
}

/** The first day on or after `from` (time ignored) the event takes place, or undefined if a one-off event is past. */
export function nextOccurrence(event: EventDay, from: Date): Date | undefined {
  const start = new Date(from.getFullYear(), from.getMonth(), from.getDate());
  if (event.year !== undefined) {
    const date = occurrenceInYear(event, event.year);
    return date >= start ? date : undefined;
  }
  const thisYear = occurrenceInYear(event, start.getFullYear());
  return thisYear >= start ? thisYear : occurrenceInYear(event, start.getFullYear() + 1);
}

function daysSinceMonday(date: Date): number {
  return (date.getDay() + DAYS_PER_WEEK - 1) % DAYS_PER_WEEK;
}

/** Matrix of weeks (Mon-Sun) covering the given month (month: 0-11), padded with adjacent days. */
export function generateMonthMatrix(year: number, month: number): CalendarDay[][] {
  const firstDay = new Date(year, month, 1);
  const lastDay = new Date(year, month + 1, 0);
  const startOffset = daysSinceMonday(firstDay);
  const endOffset = (DAYS_PER_WEEK - daysSinceMonday(lastDay) - 1) % DAYS_PER_WEEK;
  const totalDays = startOffset + lastDay.getDate() + endOffset;

  const today = new Date();
  const cursor = new Date(year, month, 1 - startOffset);
  const days: CalendarDay[] = [];

  for (let i = 0; i < totalDays; i++) {
    days.push({
      date: new Date(cursor),
      inCurrentMonth: cursor.getMonth() === month,
      isToday: isSameCalendarDay(cursor, today),
    });
    cursor.setDate(cursor.getDate() + 1);
  }

  const weeks: CalendarDay[][] = [];
  for (let i = 0; i < days.length; i += DAYS_PER_WEEK) {
    weeks.push(days.slice(i, i + DAYS_PER_WEEK));
  }
  return weeks;
}

export function formatMonthYear(year: number, month: number): string {
  const text = new Date(year, month, 1).toLocaleDateString('es-ES', { month: 'long', year: 'numeric' });
  const clean = text.replace(' de ', ' ');
  return clean.charAt(0).toUpperCase() + clean.slice(1);
}

/** Week (Mon-Sun) containing the given date. */
export function generateWeek(date: Date): CalendarDay[] {
  const startOffset = daysSinceMonday(date);
  const start = new Date(date);
  start.setDate(date.getDate() - startOffset);

  const today = new Date();
  const days: CalendarDay[] = [];
  for (let i = 0; i < DAYS_PER_WEEK; i++) {
    const d = new Date(start);
    d.setDate(start.getDate() + i);
    days.push({ date: d, inCurrentMonth: d.getMonth() === date.getMonth(), isToday: isSameCalendarDay(d, today) });
  }
  return days;
}

export function formatWeekRange(days: CalendarDay[]): string {
  const start = days[0].date;
  const end = days[6].date;
  const startMonth = monthNames[start.getMonth()].slice(0, 3).toLowerCase();
  const endMonth = monthNames[end.getMonth()].slice(0, 3).toLowerCase();

  if (start.getFullYear() === end.getFullYear()) {
    if (start.getMonth() === end.getMonth()) {
      return `${start.getDate()} – ${end.getDate()} ${endMonth} ${end.getFullYear()}`;
    }
    return `${start.getDate()} ${startMonth} – ${end.getDate()} ${endMonth} ${end.getFullYear()}`;
  }
  return `${start.getDate()} ${startMonth} ${start.getFullYear()} – ${end.getDate()} ${endMonth} ${end.getFullYear()}`;
}

export const monthNames = [
  'Enero',
  'Febrero',
  'Marzo',
  'Abril',
  'Mayo',
  'Junio',
  'Julio',
  'Agosto',
  'Septiembre',
  'Octubre',
  'Noviembre',
  'Diciembre',
];

export function formatFullDay(date: Date): string {
  const text = date.toLocaleDateString('es-ES', { weekday: 'long', day: 'numeric', month: 'long' });
  return text.charAt(0).toUpperCase() + text.slice(1);
}

function daysFromToday(date: Date): number {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const day = new Date(date);
  day.setHours(0, 0, 0, 0);
  return Math.round((day.getTime() - today.getTime()) / MS_PER_DAY);
}

/** Short label of a day-granular date (due dates have no time): Hoy, Mañana, Ayer or "5 oct". */
export function formatDueDate(date: Date): string {
  const diff = daysFromToday(date);
  if (diff === 0) return 'Hoy';
  if (diff === 1) return 'Mañana';
  if (diff === -1) return 'Ayer';
  return date.toLocaleDateString('es-ES', { day: 'numeric', month: 'short' });
}

/** Heading of a day group: Hoy, Mañana, Ayer, "Atrasado · <day>" or the full day. */
export function formatDayHeading(date: Date): string {
  const diff = daysFromToday(date);
  if (diff === 0) return 'Hoy';
  if (diff === 1) return 'Mañana';
  if (diff === -1) return 'Ayer';
  if (diff < 0) return `Atrasado · ${formatFullDay(date)}`;
  return formatFullDay(date);
}

const DATE_INPUT_DIGITS = 2;

/** The value a date input and the date picker use: YYYY-MM-DD, in local time. */
export function toDateInputValue(date: Date): string {
  const pad = (value: number) => String(value).padStart(DATE_INPUT_DIGITS, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

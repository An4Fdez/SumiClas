const rtf = new Intl.RelativeTimeFormat('es', { numeric: 'auto' });

export function formatRelativeDate(dateISO: string): string {
  const date = new Date(dateISO);
  const diffMs = date.getTime() - Date.now();
  const diffMin = Math.round(diffMs / 60_000);
  const diffH = Math.round(diffMs / 3_600_000);
  const diffD = Math.round(diffMs / 86_400_000);

  if (Math.abs(diffMin) < 60) return rtf.format(diffMin, 'minute');
  if (Math.abs(diffH) < 24) return rtf.format(diffH, 'hour');
  if (Math.abs(diffD) < 7) return rtf.format(diffD, 'day');

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

/** Year to display an event in: the real one, except for yearly recurring events, which always navigate to the current year. */
export function visibleYearForEvent(dateISO: string, recurringAnnual: boolean): number {
  return recurringAnnual ? new Date().getFullYear() : new Date(dateISO).getFullYear();
}

/** true if an event occurs on that day; for yearly recurring events (birthdays) compares month+day only. */
export function eventOccursOnDay(eventDateISO: string, day: Date, recurringAnnual: boolean): boolean {
  const eventDate = new Date(eventDateISO);
  if (recurringAnnual) {
    return eventDate.getMonth() === day.getMonth() && eventDate.getDate() === day.getDate();
  }
  return isSameCalendarDay(eventDate, day);
}

/** Matrix of weeks (Mon-Sun) covering the given month (month: 0-11), padded with adjacent days. */
export function generateMonthMatrix(year: number, month: number): CalendarDay[][] {
  const firstDay = new Date(year, month, 1);
  const lastDay = new Date(year, month + 1, 0);
  const startOffset = (firstDay.getDay() + 6) % 7;
  const endOffset = (7 - ((lastDay.getDay() + 6) % 7) - 1 + 7) % 7;
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
  for (let i = 0; i < days.length; i += 7) {
    weeks.push(days.slice(i, i + 7));
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
  const startOffset = (date.getDay() + 6) % 7;
  const start = new Date(date);
  start.setDate(date.getDate() - startOffset);

  const today = new Date();
  const days: CalendarDay[] = [];
  for (let i = 0; i < 7; i++) {
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

import { createContext } from 'react';
import type { ICalendarEventRepository } from '@/modules/calendarEvents/domain/ICalendarEventRepository';

export const CalendarEventRepositoryContext = createContext<ICalendarEventRepository | null>(null);

import { useContext } from 'react';
import { CalendarEventRepositoryContext } from '@/context/CalendarEventRepositoryContext';

export function useCalendarEventRepository() {
  const repository = useContext(CalendarEventRepositoryContext);
  if (!repository) throw new Error('useCalendarEventRepository must be used within CalendarEventRepositoryProvider');
  return repository;
}

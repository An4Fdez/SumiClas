import { createContext } from 'react';
import type { IWeeklySummaryRepository } from '@/modules/weeklySummary/domain/IWeeklySummaryRepository';

export const WeeklySummaryRepositoryContext = createContext<IWeeklySummaryRepository | null>(null);

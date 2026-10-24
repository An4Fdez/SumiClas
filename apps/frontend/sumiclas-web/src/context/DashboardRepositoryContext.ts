import { createContext } from 'react';
import type { IDashboardRepository } from '@/modules/dashboard/domain/IDashboardRepository';

export const DashboardRepositoryContext = createContext<IDashboardRepository | null>(null);

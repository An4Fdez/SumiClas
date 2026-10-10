import { createContext } from 'react';
import type { IClassExpenseRepository } from '@/modules/classExpenses/domain/IClassExpenseRepository';

export const ClassExpenseRepositoryContext = createContext<IClassExpenseRepository | null>(null);

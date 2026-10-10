import type { ClassExpense } from './ClassExpense';

export type ClassExpensePage = {
  items: ClassExpense[];
  nextCursor?: string;
};

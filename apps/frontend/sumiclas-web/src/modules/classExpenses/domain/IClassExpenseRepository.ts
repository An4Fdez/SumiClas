import type { PageRequest } from '@/modules/Shared/domain/PageRequest';
import type { ClassExpenseChanges } from './ClassExpenseChanges';
import type { ClassExpensePage } from './ClassExpensePage';
import type { NewClassExpense } from './NewClassExpense';

export interface IClassExpenseRepository {
  listByClass(classId: string, page?: PageRequest): Promise<ClassExpensePage>;
  create(expense: NewClassExpense): Promise<void>;
  update(expenseId: string, changes: ClassExpenseChanges): Promise<void>;
  remove(expenseId: string): Promise<void>;
  markAsPaid(expenseId: string, guardianId: string): Promise<void>;
  markAsUnpaid(expenseId: string, guardianId: string): Promise<void>;
}

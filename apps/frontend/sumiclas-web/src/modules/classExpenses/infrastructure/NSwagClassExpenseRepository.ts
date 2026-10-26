import { parseClassRole } from '@/lib/roles';
import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { PageRequest } from '@/modules/Shared/domain/PageRequest';
import type { ClassExpense } from '../domain/ClassExpense';
import type { ClassExpenseChanges } from '../domain/ClassExpenseChanges';
import type { ClassExpensePage } from '../domain/ClassExpensePage';
import type { IClassExpenseRepository } from '../domain/IClassExpenseRepository';
import type { NewClassExpense } from '../domain/NewClassExpense';

export class NSwagClassExpenseRepository implements IClassExpenseRepository {
  constructor(private readonly client: ApiClient) {}

  async listByClass(classId: string, page?: PageRequest): Promise<ClassExpensePage> {
    const response = await this.client.classExpenseFinderByClass(classId, page?.limit, page?.cursor);
    const items: ClassExpense[] = (response.items ?? []).map((expense) => ({
      id: expense.id,
      classId: expense.classId,
      item: expense.item,
      totalAmount: expense.totalAmount,
      relatedEvent: expense.relatedEvent ? { id: expense.relatedEvent.id, title: expense.relatedEvent.title } : undefined,
      authorId: expense.authorId,
      authorName: expense.authorName,
      authorRole: parseClassRole(expense.authorRole),
      createdAt: expense.createdAt,
      updatedAt: expense.updatedAt ?? undefined,
      isAuthor: expense.isAuthor,
      participants: (expense.participants ?? []).map((participant) => ({
        guardianId: participant.guardianId,
        fullName: participant.fullName,
        hasPaid: participant.hasPaid,
      })),
    }));
    return { items, nextCursor: response.nextCursor ?? undefined };
  }

  async create(expense: NewClassExpense): Promise<void> {
    await this.client.classExpenseCreate({
      id: expense.id,
      classId: expense.classId,
      item: expense.item,
      totalAmount: expense.totalAmount,
      participantIds: expense.participantIds,
      relatedEventId: expense.relatedEventId,
    });
  }

  async update(expenseId: string, changes: ClassExpenseChanges): Promise<void> {
    await this.client.classExpenseUpdate(expenseId, {
      item: changes.item,
      totalAmount: changes.totalAmount,
      relatedEventId: changes.relatedEventId,
    });
  }

  async remove(expenseId: string): Promise<void> {
    await this.client.classExpenseDelete(expenseId);
  }

  async markAsPaid(expenseId: string, guardianId: string): Promise<void> {
    await this.client.classExpensePaidMark(expenseId, guardianId);
  }

  async markAsUnpaid(expenseId: string, guardianId: string): Promise<void> {
    await this.client.classExpenseUnpaidMark(expenseId, guardianId);
  }
}

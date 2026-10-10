import type { ClassRole } from '@/lib/roles';
import type { ExpenseParticipant } from './ExpenseParticipant';
import type { RelatedEvent } from './RelatedEvent';

/** `totalAmount` is in euros with at most 2 decimals; every participant owes an equal share. */
export type ClassExpense = {
  id: string;
  classId: string;
  item: string;
  totalAmount: number;
  relatedEvent?: RelatedEvent;
  authorId: string;
  authorName: string;
  authorRole?: ClassRole;
  createdAt: Date;
  participants: ExpenseParticipant[];
};

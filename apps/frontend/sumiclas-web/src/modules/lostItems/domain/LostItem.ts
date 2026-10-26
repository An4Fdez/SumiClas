import type { ClassRole } from '@/lib/roles';
import type { ItemScope } from './ItemScope';
import type { ItemStatus } from './ItemStatus';

export type LostItem = {
  id: string;
  scope: ItemScope;
  classId?: string;
  status: ItemStatus;
  recovered: boolean;
  description: string;
  photoUrl?: string;
  authorId: string;
  authorName: string;
  authorRole?: ClassRole;
  createdAt: Date;
  updatedAt?: Date;
  isAuthor: boolean;
};

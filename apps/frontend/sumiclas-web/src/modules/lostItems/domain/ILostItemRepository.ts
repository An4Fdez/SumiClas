import type { ClassRole } from '@/lib/roles';

export type ItemStatus = 'lost' | 'found';
export type ItemScope = 'school' | 'class';

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
};

export type LostItemPage = {
  items: LostItem[];
  nextCursor?: string;
};

export type PageRequest = {
  limit?: number;
  cursor?: string;
};

export type NewLostItem = {
  id: string;
  scope: ItemScope;
  classId?: string;
  status: ItemStatus;
  description: string;
  photoUrl?: string;
  photoPrivacyConfirmed: boolean;
};

export interface ILostItemRepository {
  listByClass(classId: string, page?: PageRequest): Promise<LostItemPage>;
  create(item: NewLostItem): Promise<void>;
  markAsRecovered(id: string): Promise<void>;
  markAsNotRecovered(id: string): Promise<void>;
}

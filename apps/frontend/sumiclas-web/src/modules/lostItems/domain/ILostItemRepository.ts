import type { PageRequest } from '@/modules/Shared/domain/PageRequest';
import type { LostItemPage } from './LostItemPage';
import type { NewLostItem } from './NewLostItem';

export interface ILostItemRepository {
  listByClass(classId: string, page?: PageRequest): Promise<LostItemPage>;
  create(item: NewLostItem): Promise<void>;
  updateDescription(id: string, description: string): Promise<void>;
  remove(id: string): Promise<void>;
  markAsRecovered(id: string): Promise<void>;
  markAsNotRecovered(id: string): Promise<void>;
}

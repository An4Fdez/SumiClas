import type { PageRequest } from '@/modules/Shared/domain/PageRequest';
import type { LostItemClaim } from './LostItemClaim';
import type { LostItemPage } from './LostItemPage';
import type { NewLostItem } from './NewLostItem';
import type { NewLostItemClaim } from './NewLostItemClaim';

export interface ILostItemRepository {
  listByClass(classId: string, page?: PageRequest): Promise<LostItemPage>;
  create(item: NewLostItem): Promise<void>;
  updateDescription(id: string, description: string): Promise<void>;
  remove(id: string): Promise<void>;
  markAsRecovered(id: string): Promise<void>;
  markAsNotRecovered(id: string): Promise<void>;
  /** The author gets every claim of the item; anyone else only their own. Newest first. */
  listClaims(lostItemId: string): Promise<LostItemClaim[]>;
  createClaim(lostItemId: string, claim: NewLostItemClaim): Promise<void>;
  acceptClaim(lostItemId: string, claimId: string, responseMessage?: string): Promise<void>;
  rejectClaim(lostItemId: string, claimId: string): Promise<void>;
}

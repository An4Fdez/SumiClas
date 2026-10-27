import type { ClassRole } from '@/lib/roles';
import type { ClaimStatus } from './ClaimStatus';
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
  /** Set for the author only. */
  pendingClaimsCount?: number;
  /** Set for everyone but the author: the status of their latest claim, if any. */
  myClaimStatus?: ClaimStatus;
};

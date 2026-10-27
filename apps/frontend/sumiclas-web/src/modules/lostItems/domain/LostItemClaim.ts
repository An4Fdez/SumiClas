import type { ClassRole } from '@/lib/roles';
import type { ClaimStatus } from './ClaimStatus';

export type LostItemClaim = {
  id: string;
  claimantId: string;
  claimantName: string;
  claimantRole?: ClassRole;
  message: string;
  status: ClaimStatus;
  /** The author's coordination line; only present once the claim is accepted. */
  responseMessage?: string;
  createdAt: Date;
  isMine: boolean;
};

import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { ClassRole } from '@/lib/roles';
import type { ILostItemRepository } from '../domain/ILostItemRepository';
import type { ClaimStatus } from '../domain/ClaimStatus';
import type { LostItemClaim } from '../domain/LostItemClaim';
import type { NewLostItemClaim } from '../domain/NewLostItemClaim';
import type { ItemStatus } from '../domain/ItemStatus';
import type { LostItem } from '../domain/LostItem';
import type { LostItemPage } from '../domain/LostItemPage';
import type { NewLostItem } from '../domain/NewLostItem';
import type { PageRequest } from '@/modules/Shared/domain/PageRequest';

function toStatus(value: string): ItemStatus {
  return value.toLowerCase() === 'found' ? 'found' : 'lost';
}

function toRole(value: string | undefined): ClassRole | undefined {
  if (value === undefined || value === null) return undefined;
  return value.toLowerCase() === 'classrepresentative' ? 'classRepresentative' : 'participant';
}

function toClaimStatus(value: string): ClaimStatus {
  const status = value.toLowerCase();
  if (status === 'accepted') return 'accepted';
  return status === 'rejected' ? 'rejected' : 'pending';
}

function capitalize(value: string): string {
  return value.charAt(0).toUpperCase() + value.slice(1);
}

export class NSwagLostItemRepository implements ILostItemRepository {
  constructor(private readonly client: ApiClient) {}

  async listByClass(classId: string, page?: PageRequest): Promise<LostItemPage> {
    const response = await this.client.lostItemFinderByClass(classId, page?.limit, page?.cursor);
    const items: LostItem[] = (response.items ?? []).map((item) => ({
      id: item.id,
      scope: item.scope.toLowerCase() === 'school' ? 'school' : 'class',
      classId: item.classId ?? undefined,
      status: toStatus(item.status),
      recovered: item.recovered,
      description: item.description,
      photoUrl: item.photoUrl ?? undefined,
      authorId: item.authorId,
      authorName: item.authorName,
      authorRole: toRole(item.authorRole),
      createdAt: item.createdAt,
      updatedAt: item.updatedAt ?? undefined,
      isAuthor: item.isAuthor,
      pendingClaimsCount: item.pendingClaimsCount ?? undefined,
      myClaimStatus: item.myClaimStatus ? toClaimStatus(item.myClaimStatus) : undefined,
    }));
    return { items, nextCursor: response.nextCursor ?? undefined };
  }

  async create(item: NewLostItem): Promise<void> {
    await this.client.lostItemCreate({
      id: item.id,
      scope: capitalize(item.scope),
      classId: item.classId,
      status: capitalize(item.status),
      description: item.description,
      photoUrl: item.photoUrl,
      photoPrivacyConfirmed: item.photoPrivacyConfirmed,
    });
  }

  async updateDescription(id: string, description: string): Promise<void> {
    await this.client.lostItemUpdate(id, { description });
  }

  async remove(id: string): Promise<void> {
    await this.client.lostItemDelete(id);
  }

  async markAsRecovered(id: string): Promise<void> {
    await this.client.lostItemRecoveredMark(id);
  }

  async markAsNotRecovered(id: string): Promise<void> {
    await this.client.lostItemNotRecoveredMark(id);
  }

  async listClaims(lostItemId: string): Promise<LostItemClaim[]> {
    const response = await this.client.lostItemClaimFinderByItem(lostItemId);
    return (response ?? []).map((claim) => ({
      id: claim.id,
      claimantId: claim.claimantId,
      claimantName: claim.claimantName,
      claimantRole: toRole(claim.claimantRole),
      message: claim.message,
      status: toClaimStatus(claim.status),
      responseMessage: claim.responseMessage ?? undefined,
      createdAt: claim.createdAt,
      isMine: claim.isMine,
    }));
  }

  async createClaim(lostItemId: string, claim: NewLostItemClaim): Promise<void> {
    await this.client.lostItemClaimCreate(lostItemId, { id: claim.id, message: claim.message });
  }

  async acceptClaim(lostItemId: string, claimId: string, responseMessage?: string): Promise<void> {
    await this.client.lostItemClaimAccept(lostItemId, claimId, { responseMessage });
  }

  async rejectClaim(lostItemId: string, claimId: string): Promise<void> {
    await this.client.lostItemClaimReject(lostItemId, claimId);
  }
}

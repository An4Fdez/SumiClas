import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { ClassRole } from '@/lib/roles';
import type {
  ILostItemRepository,
  ItemStatus,
  LostItem,
  LostItemPage,
  NewLostItem,
  PageRequest,
} from '../domain/ILostItemRepository';

function toStatus(value: string): ItemStatus {
  return value.toLowerCase() === 'found' ? 'found' : 'lost';
}

function toRole(value: string | undefined): ClassRole | undefined {
  if (value === undefined || value === null) return undefined;
  return value.toLowerCase() === 'classrepresentative' ? 'classRepresentative' : 'participant';
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

  async markAsRecovered(id: string): Promise<void> {
    await this.client.lostItemRecoveredMark(id);
  }

  async markAsNotRecovered(id: string): Promise<void> {
    await this.client.lostItemNotRecoveredMark(id);
  }
}

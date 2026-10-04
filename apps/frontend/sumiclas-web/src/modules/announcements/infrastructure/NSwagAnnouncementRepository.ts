import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { ClassRole } from '@/lib/roles';
import type { IAnnouncementRepository } from '../domain/IAnnouncementRepository';
import type { Announcement } from '../domain/Announcement';
import type { AnnouncementPage } from '../domain/AnnouncementPage';
import type { NewAnnouncement } from '../domain/NewAnnouncement';
import type { PageRequest } from '@/modules/Shared/domain/PageRequest';
import type { Priority } from '../domain/Priority';

function toPriority(value: string): Priority {
  const priority = value.toLowerCase();
  return priority === 'low' || priority === 'high' ? priority : 'normal';
}

function toRole(value: string | undefined): ClassRole | undefined {
  if (value === undefined || value === null) return undefined;
  return value.toLowerCase() === 'classrepresentative' ? 'classRepresentative' : 'participant';
}

function capitalize(value: string): string {
  return value.charAt(0).toUpperCase() + value.slice(1);
}

export class NSwagAnnouncementRepository implements IAnnouncementRepository {
  constructor(private readonly client: ApiClient) {}

  async listByClass(classId: string, page?: PageRequest): Promise<AnnouncementPage> {
    const response = await this.client.announcementFinderByClass(classId, page?.limit, page?.cursor);
    const items: Announcement[] = (response.items ?? []).map((item) => ({
      id: item.id,
      scope: item.scope.toLowerCase() === 'school' ? 'school' : 'class',
      classId: item.classId ?? undefined,
      title: item.title,
      body: item.body,
      priority: toPriority(item.priority),
      authorId: item.authorId,
      authorName: item.authorName,
      authorRole: toRole(item.authorRole),
      createdAt: item.createdAt,
      relatedEvent: item.relatedEvent ? { id: item.relatedEvent.id, title: item.relatedEvent.title } : undefined,
      read: item.read,
    }));
    return { items, nextCursor: response.nextCursor ?? undefined };
  }

  async create(announcement: NewAnnouncement): Promise<void> {
    await this.client.announcementCreate({
      id: announcement.id,
      scope: capitalize(announcement.scope),
      classId: announcement.classId,
      title: announcement.title,
      body: announcement.body,
      priority: capitalize(announcement.priority),
      relatedEventId: announcement.relatedEventId,
    });
  }

  async markAsRead(id: string): Promise<void> {
    await this.client.announcementReadMark(id);
  }
}

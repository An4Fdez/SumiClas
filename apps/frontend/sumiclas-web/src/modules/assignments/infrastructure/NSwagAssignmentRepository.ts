import type { ClassRole } from '@/lib/roles';
import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import { toApiDateOnly } from '@/modules/Shared/Infrastructure/apiDate';
import type { PageRequest } from '@/modules/Shared/domain/PageRequest';
import type { Assignment } from '../domain/Assignment';
import type { AssignmentChanges } from '../domain/AssignmentChanges';
import type { AssignmentPage } from '../domain/AssignmentPage';
import type { IAssignmentRepository } from '../domain/IAssignmentRepository';
import type { NewAssignment } from '../domain/NewAssignment';

function toRole(value: string | undefined): ClassRole | undefined {
  const role = value?.toLowerCase();
  if (role === 'classrepresentative') return 'classRepresentative';
  if (role === 'participant') return 'participant';
  return undefined;
}

export class NSwagAssignmentRepository implements IAssignmentRepository {
  constructor(private readonly client: ApiClient) {}

  async listByClass(classId: string, page?: PageRequest): Promise<AssignmentPage> {
    const response = await this.client.assignmentFinderByClass(classId, page?.limit, page?.cursor);
    const items: Assignment[] = (response.items ?? []).map((item) => ({
      id: item.id,
      classId: item.classId,
      subjectId: item.subjectId,
      subjectLabel: item.subjectLabel,
      title: item.title,
      description: item.description ?? undefined,
      dueDate: item.dueDate,
      authorId: item.authorId,
      authorName: item.authorName,
      authorRole: toRole(item.authorRole),
      isProposal: item.isProposal,
      done: item.done,
      createdAt: item.createdAt,
      updatedAt: item.updatedAt ?? undefined,
      isAuthor: item.isAuthor,
      comments: (item.comments ?? []).map((comment) => ({
        id: comment.id,
        authorId: comment.authorId,
        authorName: comment.authorName,
        authorRole: toRole(comment.authorRole),
        text: comment.text,
        createdAt: comment.createdAt,
      })),
    }));
    return { items, nextCursor: response.nextCursor ?? undefined };
  }

  async create(assignment: NewAssignment): Promise<void> {
    await this.client.assignmentCreate({
      id: assignment.id,
      classId: assignment.classId,
      subjectId: assignment.subjectId,
      title: assignment.title,
      description: assignment.description,
      dueDate: toApiDateOnly(assignment.dueDate),
    });
  }

  async update(assignmentId: string, changes: AssignmentChanges): Promise<void> {
    await this.client.assignmentUpdate(assignmentId, {
      subjectId: changes.subjectId,
      title: changes.title,
      description: changes.description,
      dueDate: toApiDateOnly(changes.dueDate),
    });
  }

  async remove(assignmentId: string): Promise<void> {
    await this.client.assignmentDelete(assignmentId);
  }

  async addComment(assignmentId: string, text: string): Promise<void> {
    await this.client.assignmentCommentAdd(assignmentId, { text });
  }

  async markAsDone(assignmentId: string): Promise<void> {
    await this.client.assignmentDoneMark(assignmentId);
  }

  async markAsNotDone(assignmentId: string): Promise<void> {
    await this.client.assignmentDoneUnmark(assignmentId);
  }
}

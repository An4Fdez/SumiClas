import type { PageRequest } from '@/modules/Shared/domain/PageRequest';
import type { AssignmentChanges } from './AssignmentChanges';
import type { AssignmentPage } from './AssignmentPage';
import type { NewAssignment } from './NewAssignment';

export interface IAssignmentRepository {
  listByClass(classId: string, page?: PageRequest): Promise<AssignmentPage>;
  create(assignment: NewAssignment): Promise<void>;
  update(assignmentId: string, changes: AssignmentChanges): Promise<void>;
  remove(assignmentId: string): Promise<void>;
  addComment(assignmentId: string, text: string): Promise<void>;
  markAsDone(assignmentId: string): Promise<void>;
  markAsNotDone(assignmentId: string): Promise<void>;
}

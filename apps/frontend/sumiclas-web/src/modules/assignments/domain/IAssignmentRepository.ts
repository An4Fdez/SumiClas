import type { PageRequest } from '@/modules/Shared/domain/PageRequest';
import type { AssignmentPage } from './AssignmentPage';
import type { NewAssignment } from './NewAssignment';

export interface IAssignmentRepository {
  listByClass(classId: string, page?: PageRequest): Promise<AssignmentPage>;
  create(assignment: NewAssignment): Promise<void>;
  addComment(assignmentId: string, text: string): Promise<void>;
}

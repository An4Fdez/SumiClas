import type { ClassRole } from '@/lib/roles';
import type { AssignmentComment } from './AssignmentComment';

export type Assignment = {
  id: string;
  classId: string;
  subjectId: string;
  subjectLabel: string;
  title: string;
  description?: string;
  dueDate: Date;
  authorId: string;
  authorName: string;
  authorRole?: ClassRole;
  isProposal: boolean;
  done: boolean;
  createdAt: Date;
  comments: AssignmentComment[];
};

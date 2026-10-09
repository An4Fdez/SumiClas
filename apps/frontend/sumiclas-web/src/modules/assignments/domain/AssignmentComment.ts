import type { ClassRole } from '@/lib/roles';

export type AssignmentComment = {
  id: string;
  authorId: string;
  authorName: string;
  authorRole?: ClassRole;
  text: string;
  createdAt: Date;
};

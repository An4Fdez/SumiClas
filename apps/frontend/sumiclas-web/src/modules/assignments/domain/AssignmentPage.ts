import type { Assignment } from './Assignment';

export type AssignmentPage = {
  items: Assignment[];
  nextCursor?: string;
};

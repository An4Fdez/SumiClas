import type { ClassRole } from '@/lib/roles';

/** A guardian that belongs to a class. Only the name and the role are exposed: never an email or data of children. */
export type ClassMember = {
  userId: string;
  fullName: string;
  role?: ClassRole;
};

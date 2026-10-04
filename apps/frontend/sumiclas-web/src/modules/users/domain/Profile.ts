import type { ClassRoleAssignment } from './ClassRoleAssignment';

export type Profile = {
  id: string;
  fullName: string;
  email: string;
  classRoles: ClassRoleAssignment[];
};

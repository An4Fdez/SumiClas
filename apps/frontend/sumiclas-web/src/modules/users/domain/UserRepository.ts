import type { ClassRole } from '@/lib/roles';

export type ClassRoleAssignment = {
  classId: string;
  role: ClassRole;
};

export type Profile = {
  id: string;
  fullName: string;
  email: string;
  classRoles: ClassRoleAssignment[];
};

export type UserRegistration = {
  id: string;
  fullName: string;
  email: string;
};

export interface UserRepository {
  register(user: UserRegistration): Promise<void>;
  getCurrent(): Promise<Profile>;
}

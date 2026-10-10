import type { ClassMember } from './ClassMember';
import type { Profile } from './Profile';
import type { UserRegistration } from './UserRegistration';

export interface IUserRepository {
  register(user: UserRegistration): Promise<void>;
  getCurrent(): Promise<Profile>;
  listClassMembers(classId: string): Promise<ClassMember[]>;
  assignClassRepresentative(userId: string, classId: string): Promise<void>;
  resignClassRepresentative(classId: string): Promise<void>;
}

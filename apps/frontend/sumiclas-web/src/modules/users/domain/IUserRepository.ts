import type { ClassMember } from './ClassMember';
import type { NewUser } from './NewUser';
import type { Profile } from './Profile';

export interface IUserRepository {
  getCurrent(): Promise<Profile>;
  listClassMembers(classId: string): Promise<ClassMember[]>;
  assignClassRepresentative(userId: string, classId: string): Promise<void>;
  resignClassRepresentative(classId: string): Promise<void>;
  register(newUser: NewUser): Promise<void>;
}

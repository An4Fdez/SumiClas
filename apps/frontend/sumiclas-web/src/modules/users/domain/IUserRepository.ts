import type { ClassMember } from './ClassMember';
import type { NewUser } from './NewUser';
import type { Profile } from './Profile';

export interface IUserRepository {
  getCurrent(): Promise<Profile>;
  listClassMembers(classId: string): Promise<ClassMember[]>;
  assignClassRepresentative(userId: string, classId: string): Promise<void>;
  resignClassRepresentative(classId: string): Promise<void>;
  register(newUser: NewUser): Promise<void>;
  changeName(fullName: string): Promise<void>;
  /** The server revokes every session of the user: the caller must end the local one. */
  changePassword(currentPassword: string, newPassword: string): Promise<void>;
}

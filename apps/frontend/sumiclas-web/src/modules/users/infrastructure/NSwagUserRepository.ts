import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import { getSession, setSession } from '@/lib/tokenStore';
import { parseClassRole, type ClassRole } from '@/lib/roles';
import type { ClassMember } from '../domain/ClassMember';
import type { IUserRepository } from '../domain/IUserRepository';
import type { NewUser } from '../domain/NewUser';
import type { Profile } from '../domain/Profile';

function toClassRole(role: string): ClassRole {
  return role.toLowerCase() === 'classrepresentative' ? 'classRepresentative' : 'participant';
}

export class NSwagUserRepository implements IUserRepository {
  constructor(private readonly client: ApiClient) {}

  async getCurrent(): Promise<Profile> {
    const profile = await this.client.userProfileFinderCurrent();
    return {
      id: profile.id,
      fullName: profile.fullName,
      email: profile.email,
      classRoles: (profile.classRoles ?? []).map((assignment) => ({
        classId: assignment.classId,
        role: toClassRole(assignment.role),
      })),
    };
  }

  async listClassMembers(classId: string): Promise<ClassMember[]> {
    const members = await this.client.userClassMemberFinderByClass(classId);
    return (members ?? []).map((member) => ({
      userId: member.userId,
      fullName: member.fullName,
      role: parseClassRole(member.classRole),
    }));
  }

  async assignClassRepresentative(userId: string, classId: string): Promise<void> {
    await this.client.userClassRepresentativeAssign(userId, classId);
  }

  async resignClassRepresentative(classId: string): Promise<void> {
    await this.client.userClassRepresentativeResign(classId);
  }

  async register(newUser: NewUser): Promise<void> {
    await this.client.userRegister({
      id: newUser.id,
      fullName: newUser.fullName,
      email: newUser.email,
      password: newUser.password,
    });
  }

  async changeName(fullName: string): Promise<void> {
    await this.client.userNameChange({ fullName });
    const session = getSession();
    if (session) setSession({ ...session, fullName });
  }

  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    await this.client.userPasswordChange({ currentPassword, newPassword });
  }
}

import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import { parseClassRole, type ClassRole } from '@/lib/roles';
import type { ClassMember } from '../domain/ClassMember';
import type { IUserRepository } from '../domain/IUserRepository';
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
}

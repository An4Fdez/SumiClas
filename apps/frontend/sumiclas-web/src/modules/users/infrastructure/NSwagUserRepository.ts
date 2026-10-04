import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { ClassRole } from '@/lib/roles';
import type { Profile, UserRegistration, IUserRepository } from '../domain/IUserRepository';

function toClassRole(role: string): ClassRole {
  return role.toLowerCase() === 'classrepresentative' ? 'classRepresentative' : 'participant';
}

export class NSwagUserRepository implements IUserRepository {
  constructor(private readonly client: ApiClient) {}

  async register(user: UserRegistration): Promise<void> {
    await this.client.userRegister({ id: user.id, fullName: user.fullName, email: user.email });
  }

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
}

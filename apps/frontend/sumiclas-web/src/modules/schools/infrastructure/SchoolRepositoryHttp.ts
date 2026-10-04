import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { NewSchool, School, ISchoolRepository } from '../domain/ISchoolRepository';

export class SchoolRepositoryHttp implements ISchoolRepository {
  constructor(private readonly client: ApiClient) {}

  async searchByName(name: string): Promise<School[]> {
    const schools = await this.client.schoolFinderByName(name);
    return schools.map((school) => ({ id: school.id, name: school.name, address: school.address ?? undefined }));
  }

  async register(school: NewSchool): Promise<void> {
    await this.client.schoolRegister({
      id: school.id,
      name: school.name,
      officialCode: school.officialCode,
      address: school.address,
    });
  }
}

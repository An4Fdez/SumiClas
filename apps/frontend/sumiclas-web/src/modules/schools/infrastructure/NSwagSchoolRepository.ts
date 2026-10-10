import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { SchoolResponse } from '@/modules/Shared/Infrastructure/GeneratedApiClient';
import type { ISchoolRepository } from '../domain/ISchoolRepository';
import type { School } from '../domain/School';

function toSchool(school: SchoolResponse): School {
  return {
    id: school.id,
    name: school.name,
    officialCode: school.officialCode,
    provinceId: school.provinceId,
    provinceName: school.provinceName,
    localityId: school.localityId,
    localityName: school.localityName,
    postalCode: school.postalCode ?? undefined,
    address: school.address ?? undefined,
    phone: school.phone ?? undefined,
  };
}

export class NSwagSchoolRepository implements ISchoolRepository {
  constructor(private readonly client: ApiClient) {}

  async findByLocality(localityId: string, name?: string): Promise<School[]> {
    const schools = await this.client.schoolFinderByLocality(localityId, name || undefined);
    return schools.map(toSchool);
  }

  async findById(schoolId: string): Promise<School> {
    return toSchool(await this.client.schoolFinderById(schoolId));
  }
}

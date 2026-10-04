import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { IClassRepository } from '../domain/IClassRepository';
import type { NewSchoolClass } from '../domain/NewSchoolClass';
import type { SchoolClass } from '../domain/SchoolClass';

export class NSwagClassRepository implements IClassRepository {
  constructor(private readonly client: ApiClient) {}

  async listBySchoolYear(schoolYearId: string): Promise<SchoolClass[]> {
    const classes = await this.client.classFinderBySchoolYear(schoolYearId);
    return classes.map((schoolClass) => ({ id: schoolClass.id, name: schoolClass.name }));
  }

  async create(schoolClass: NewSchoolClass): Promise<void> {
    await this.client.classCreate({ id: schoolClass.id, schoolYearId: schoolClass.schoolYearId, name: schoolClass.name });
  }
}

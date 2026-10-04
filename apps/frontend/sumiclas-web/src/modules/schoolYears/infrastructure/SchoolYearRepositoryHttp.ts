import { toApiDateOnly } from '@/modules/Shared/Infrastructure/apiDate';
import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { NewSchoolYear, SchoolYear, ISchoolYearRepository } from '../domain/ISchoolYearRepository';

export class SchoolYearRepositoryHttp implements ISchoolYearRepository {
  constructor(private readonly client: ApiClient) {}

  async listActive(schoolId: string): Promise<SchoolYear[]> {
    const schoolYears = await this.client.schoolYearFinderBySchool(schoolId, 'Active');
    return schoolYears.map((schoolYear) => ({ id: schoolYear.id, label: schoolYear.label }));
  }

  async create(schoolYear: NewSchoolYear): Promise<void> {
    await this.client.schoolYearCreate({
      id: schoolYear.id,
      schoolId: schoolYear.schoolId,
      label: schoolYear.label,
      startDate: toApiDateOnly(schoolYear.startDate),
      endDate: toApiDateOnly(schoolYear.endDate),
    });
  }
}

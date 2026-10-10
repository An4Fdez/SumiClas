import type { School } from './School';

export interface ISchoolRepository {
  findByLocality(localityId: string, name?: string): Promise<School[]>;
  findById(schoolId: string): Promise<School>;
}

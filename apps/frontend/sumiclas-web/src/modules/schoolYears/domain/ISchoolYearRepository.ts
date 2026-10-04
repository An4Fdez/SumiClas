import type { NewSchoolYear } from './NewSchoolYear';
import type { SchoolYear } from './SchoolYear';

export interface ISchoolYearRepository {
  listActive(schoolId: string): Promise<SchoolYear[]>;
  create(schoolYear: NewSchoolYear): Promise<void>;
}

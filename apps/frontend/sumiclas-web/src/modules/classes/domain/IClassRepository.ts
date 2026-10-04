import type { NewSchoolClass } from './NewSchoolClass';
import type { SchoolClass } from './SchoolClass';

export interface IClassRepository {
  listBySchoolYear(schoolYearId: string): Promise<SchoolClass[]>;
  create(schoolClass: NewSchoolClass): Promise<void>;
}

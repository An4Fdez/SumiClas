import type { NewSchool } from './NewSchool';
import type { School } from './School';

export interface ISchoolRepository {
  searchByName(name: string): Promise<School[]>;
  register(school: NewSchool): Promise<void>;
}

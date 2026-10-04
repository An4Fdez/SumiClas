import type { Child } from './Child';
import type { NewStudent } from './NewStudent';

export interface IStudentRepository {
  listMine(): Promise<Child[]>;
  register(student: NewStudent): Promise<void>;
}

import type { Child } from './Child';
import type { NewStudent } from './NewStudent';
import type { StudentName } from './StudentName';

export interface IStudentRepository {
  listMine(): Promise<Child[]>;
  register(student: NewStudent): Promise<void>;
  changeName(studentId: string, name: StudentName): Promise<void>;
}

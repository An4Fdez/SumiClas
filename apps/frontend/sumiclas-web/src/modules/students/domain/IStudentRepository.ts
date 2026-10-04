import type { GuardianRole } from '@/lib/auth';
import type { Child } from './Child';

export type NewStudent = {
  id: string;
  firstName: string;
  firstSurname: string;
  secondSurname?: string;
  guardianRole: GuardianRole;
};

export interface IStudentRepository {
  listMine(): Promise<Child[]>;
  register(student: NewStudent): Promise<void>;
}

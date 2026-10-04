import type { GuardianRole } from '@/lib/auth';

export type NewStudent = {
  id: string;
  firstName: string;
  firstSurname: string;
  secondSurname?: string;
  guardianRole: GuardianRole;
};

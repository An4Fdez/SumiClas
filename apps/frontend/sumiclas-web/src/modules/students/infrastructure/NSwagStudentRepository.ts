import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { Child } from '../domain/Child';
import type { IStudentRepository } from '../domain/IStudentRepository';
import type { NewStudent } from '../domain/NewStudent';
import type { StudentName } from '../domain/StudentName';

/** The API already sends the initial with its dot ("P."); only add it if it is missing. */
function formatStudentName(firstName: string, surnameInitial: string): string {
  const initial = surnameInitial.endsWith('.') ? surnameInitial : `${surnameInitial}.`;
  return `${firstName} ${initial}`;
}

const GUARDIAN_ROLE_BY_UI_ROLE = { father: 'Father', mother: 'Mother', guardian: 'Guardian' } as const;

export class NSwagStudentRepository implements IStudentRepository {
  constructor(private readonly client: ApiClient) {}

  async listMine(): Promise<Child[]> {
    const students = await this.client.studentFinderByCurrentGuardian();
    return students.map((student) => ({
      id: student.studentId,
      firstName: student.firstName,
      studentName: formatStudentName(student.firstName, student.surnameInitial),
      classId: student.classId ?? undefined,
      className: student.className ?? undefined,
      schoolYearId: student.schoolYearId ?? undefined,
      schoolId: student.schoolId ?? undefined,
      schoolName: student.schoolName ?? undefined,
    }));
  }

  async register(student: NewStudent): Promise<void> {
    await this.client.studentRegister({
      id: student.id,
      firstName: student.firstName,
      firstSurname: student.firstSurname,
      secondSurname: student.secondSurname,
      firstGuardianRole: GUARDIAN_ROLE_BY_UI_ROLE[student.guardianRole],
    });
  }

  async changeName(studentId: string, name: StudentName): Promise<void> {
    await this.client.studentNameChange(studentId, {
      firstName: name.firstName,
      firstSurname: name.firstSurname,
      secondSurname: name.secondSurname,
    });
  }
}

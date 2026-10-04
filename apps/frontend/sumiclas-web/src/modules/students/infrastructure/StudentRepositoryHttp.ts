import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { Child } from '../domain/Child';
import type { StudentRepository } from '../domain/StudentRepository';

/** The API already sends the initial with its dot ("P."); only add it if it is missing. */
function formatStudentName(firstName: string, surnameInitial: string): string {
  const initial = surnameInitial.endsWith('.') ? surnameInitial : `${surnameInitial}.`;
  return `${firstName} ${initial}`;
}

export class StudentRepositoryHttp implements StudentRepository {
  constructor(private readonly client: ApiClient) {}

  async listMine(): Promise<Child[]> {
    const students = await this.client.studentFinderByCurrentGuardian();
    return students.map((student) => ({
      id: student.studentId,
      studentName: formatStudentName(student.firstName, student.surnameInitial),
      classId: student.classId ?? undefined,
      className: student.className ?? undefined,
      schoolYearId: student.schoolYearId ?? undefined,
      schoolId: student.schoolId ?? undefined,
      schoolName: student.schoolName ?? undefined,
    }));
  }
}

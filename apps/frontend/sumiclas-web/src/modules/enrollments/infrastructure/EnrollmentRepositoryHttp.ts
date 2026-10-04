import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { IEnrollmentRepository, NewEnrollment } from '../domain/IEnrollmentRepository';

export class EnrollmentRepositoryHttp implements IEnrollmentRepository {
  constructor(private readonly client: ApiClient) {}

  async create(enrollment: NewEnrollment): Promise<void> {
    await this.client.enrollmentCreate({
      id: enrollment.id,
      studentId: enrollment.studentId,
      classId: enrollment.classId,
    });
  }
}

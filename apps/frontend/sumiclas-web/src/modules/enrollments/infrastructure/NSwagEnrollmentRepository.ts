import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { IEnrollmentRepository } from '../domain/IEnrollmentRepository';
import type { NewEnrollment } from '../domain/NewEnrollment';

export class NSwagEnrollmentRepository implements IEnrollmentRepository {
  constructor(private readonly client: ApiClient) {}

  async create(enrollment: NewEnrollment): Promise<void> {
    await this.client.enrollmentCreate({
      id: enrollment.id,
      studentId: enrollment.studentId,
      classId: enrollment.classId,
    });
  }
}

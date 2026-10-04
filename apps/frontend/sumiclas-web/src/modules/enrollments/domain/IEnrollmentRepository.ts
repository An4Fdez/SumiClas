import type { NewEnrollment } from './NewEnrollment';

export interface IEnrollmentRepository {
  create(enrollment: NewEnrollment): Promise<void>;
}

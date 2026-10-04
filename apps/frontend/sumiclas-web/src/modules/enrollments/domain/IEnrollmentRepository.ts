export type NewEnrollment = {
  id: string;
  studentId: string;
  classId: string;
};

export interface IEnrollmentRepository {
  create(enrollment: NewEnrollment): Promise<void>;
}

export type NewAssignment = {
  id: string;
  classId: string;
  subjectId: string;
  title: string;
  description?: string;
  dueDate: Date;
};

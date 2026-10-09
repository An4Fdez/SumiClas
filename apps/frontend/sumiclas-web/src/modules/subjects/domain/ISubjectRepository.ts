import type { NewSubject } from './NewSubject';
import type { Subject } from './Subject';

export interface ISubjectRepository {
  listByClass(classId: string): Promise<Subject[]>;
  create(subject: NewSubject): Promise<void>;
}

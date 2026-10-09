import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { ISubjectRepository } from '../domain/ISubjectRepository';
import type { NewSubject } from '../domain/NewSubject';
import type { Subject } from '../domain/Subject';

export class NSwagSubjectRepository implements ISubjectRepository {
  constructor(private readonly client: ApiClient) {}

  async listByClass(classId: string): Promise<Subject[]> {
    const subjects = await this.client.subjectFinderByClass(classId);
    return (subjects ?? []).map((subject) => ({ id: subject.id, classId: subject.classId, label: subject.label }));
  }

  async create(subject: NewSubject): Promise<void> {
    await this.client.subjectCreate({ id: subject.id, classId: subject.classId, label: subject.label });
  }
}

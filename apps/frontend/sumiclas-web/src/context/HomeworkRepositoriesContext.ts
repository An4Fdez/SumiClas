import { createContext } from 'react';
import type { IAssignmentRepository } from '@/modules/assignments/domain/IAssignmentRepository';
import type { ISubjectRepository } from '@/modules/subjects/domain/ISubjectRepository';

export type HomeworkRepositories = {
  assignments: IAssignmentRepository;
  subjects: ISubjectRepository;
};

export const HomeworkRepositoriesContext = createContext<HomeworkRepositories | null>(null);

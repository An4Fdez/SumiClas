import { createContext } from 'react';
import type { IStudentRepository } from '@/modules/students/domain/IStudentRepository';

export const StudentRepositoryContext = createContext<IStudentRepository | null>(null);

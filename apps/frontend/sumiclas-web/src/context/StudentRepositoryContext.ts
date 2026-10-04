import { createContext } from 'react';
import type { StudentRepository } from '@/modules/students/domain/StudentRepository';

export const StudentRepositoryContext = createContext<StudentRepository | null>(null);

import { createContext } from 'react';
import type { IClassRepository } from '@/modules/classes/domain/IClassRepository';
import type { IEnrollmentRepository } from '@/modules/enrollments/domain/IEnrollmentRepository';
import type { ISchoolRepository } from '@/modules/schools/domain/ISchoolRepository';
import type { ISchoolYearRepository } from '@/modules/schoolYears/domain/ISchoolYearRepository';

export type OnboardingRepositories = {
  schools: ISchoolRepository;
  schoolYears: ISchoolYearRepository;
  classes: IClassRepository;
  enrollments: IEnrollmentRepository;
};

export const OnboardingRepositoriesContext = createContext<OnboardingRepositories | null>(null);

import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { NSwagClassRepository } from '@/modules/classes/infrastructure/NSwagClassRepository';
import { NSwagEnrollmentRepository } from '@/modules/enrollments/infrastructure/NSwagEnrollmentRepository';
import { NSwagSchoolRepository } from '@/modules/schools/infrastructure/NSwagSchoolRepository';
import { NSwagSchoolYearRepository } from '@/modules/schoolYears/infrastructure/NSwagSchoolYearRepository';
import { OnboardingRepositoriesContext, type OnboardingRepositories } from './OnboardingRepositoriesContext';

export function OnboardingRepositoriesProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repositories = useMemo<OnboardingRepositories>(
    () => ({
      schools: new NSwagSchoolRepository(client),
      schoolYears: new NSwagSchoolYearRepository(client),
      classes: new NSwagClassRepository(client),
      enrollments: new NSwagEnrollmentRepository(client),
    }),
    [client],
  );

  return <OnboardingRepositoriesContext.Provider value={repositories}>{children}</OnboardingRepositoriesContext.Provider>;
}

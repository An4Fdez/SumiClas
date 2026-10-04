import { useMemo, type ReactNode } from 'react';
import { useApiClient } from '@/hooks/useApiClient';
import { ClassRepositoryHttp } from '@/modules/classes/infrastructure/ClassRepositoryHttp';
import { EnrollmentRepositoryHttp } from '@/modules/enrollments/infrastructure/EnrollmentRepositoryHttp';
import { SchoolRepositoryHttp } from '@/modules/schools/infrastructure/SchoolRepositoryHttp';
import { SchoolYearRepositoryHttp } from '@/modules/schoolYears/infrastructure/SchoolYearRepositoryHttp';
import { OnboardingRepositoriesContext, type OnboardingRepositories } from './OnboardingRepositoriesContext';

export function OnboardingRepositoriesProvider({ children }: { children: ReactNode }) {
  const client = useApiClient();
  const repositories = useMemo<OnboardingRepositories>(
    () => ({
      schools: new SchoolRepositoryHttp(client),
      schoolYears: new SchoolYearRepositoryHttp(client),
      classes: new ClassRepositoryHttp(client),
      enrollments: new EnrollmentRepositoryHttp(client),
    }),
    [client],
  );

  return <OnboardingRepositoriesContext.Provider value={repositories}>{children}</OnboardingRepositoriesContext.Provider>;
}

import { useContext } from 'react';
import { OnboardingRepositoriesContext } from '@/context/OnboardingRepositoriesContext';

export function useOnboardingRepositories() {
  const repositories = useContext(OnboardingRepositoriesContext);
  if (!repositories) throw new Error('useOnboardingRepositories must be used within OnboardingRepositoriesProvider');
  return repositories;
}

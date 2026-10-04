import { useCallback, useEffect, useState } from 'react';
import type { WizardIds } from '@/hooks/useWizardIds';
import { useOnboardingRepositories } from '@/hooks/useOnboardingRepositories';
import { getApiErrorMessage } from '@/lib/apiError';
import { currentSchoolYear } from '@/lib/schoolYear';
import type { SchoolClass } from '@/modules/classes/domain/SchoolClass';
import type { SchoolYear } from '@/modules/schoolYears/domain/SchoolYear';

type State =
  | { status: 'loading' }
  | { status: 'ready'; schoolYear: SchoolYear; classes: SchoolClass[] }
  | { status: 'error'; message: string };

/**
 * Resolves the active school year of a school (creating the current one when the school has none) and lists its
 * classes. It always lists first, so retrying after a lost response never creates a second school year.
 */
export function useSchoolClasses(schoolId: string | undefined, ids: WizardIds) {
  const { schoolYears, classes } = useOnboardingRepositories();
  const [state, setState] = useState<State>({ status: 'loading' });
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    if (!schoolId) return;
    let cancelled = false;
    setState({ status: 'loading' });

    (async () => {
      let schoolYear = (await schoolYears.listActive(schoolId))[0];
      if (!schoolYear) {
        const { label, startDate, endDate } = currentSchoolYear();
        const id = ids.take(`schoolYear:${schoolId}`);
        await schoolYears.create({ id, schoolId, label, startDate, endDate });
        ids.consume(`schoolYear:${schoolId}`);
        schoolYear = { id, label };
      }
      return { schoolYear, classes: await classes.listBySchoolYear(schoolYear.id) };
    })()
      .then((result) => {
        if (!cancelled) setState({ status: 'ready', ...result });
      })
      .catch((error: unknown) => {
        if (!cancelled) setState({ status: 'error', message: getApiErrorMessage(error) });
      });

    return () => {
      cancelled = true;
    };
  }, [schoolId, schoolYears, classes, ids, reloadKey]);

  const reload = useCallback(() => setReloadKey((key) => key + 1), []);

  return { state, reload };
}

import { useEffect, useMemo, useState, type ReactNode } from 'react';
import type { DashboardSummary } from '@/data/appMock';
import { useStudentRepository } from '@/hooks/useStudentRepository';
import { useUserRepository } from '@/hooks/useUserRepository';
import { getApiErrorMessage, getApiErrorStatus } from '@/lib/apiError';
import type { ClassRole } from '@/lib/roles';
import type { Child } from '@/modules/students/domain/Child';
import type { Profile } from '@/modules/users/domain/IUserRepository';
import { ChildContext, NO_CLASS_LABEL, type ChildContextValue, type ChildView } from './ChildContext';

type LoadState =
  | { status: 'loading' }
  | { status: 'ready'; students: Child[]; profile: Profile }
  | { status: 'error'; message: string; httpStatus: number | undefined };

/** The API has no dashboard summary yet (T4), so every counter starts empty. */
const EMPTY_SUMMARY: DashboardSummary = {
  announcements: { new: 0, urgent: false },
  calendar: { next: null, daysUntil: null },
  homework: { pending: 0, nextDueDate: null },
  expenses: { pending: false },
  lostAndFound: { hasUpdates: false },
};

const noop = () => {};

function toView(child: Child): ChildView {
  return {
    ...child,
    className: child.className ?? NO_CLASS_LABEL,
    schoolName: child.schoolName ?? '',
  };
}

export function ChildProvider({ children }: { children: ReactNode }) {
  const studentRepository = useStudentRepository();
  const userRepository = useUserRepository();
  const [state, setState] = useState<LoadState>({ status: 'loading' });
  const [reloadKey, setReloadKey] = useState(0);
  const [selectedChildId, setSelectedChildId] = useState('');

  useEffect(() => {
    let cancelled = false;
    setState({ status: 'loading' });
    Promise.all([studentRepository.listMine(), userRepository.getCurrent()])
      .then(([students, profile]) => {
        if (!cancelled) setState({ status: 'ready', students, profile });
      })
      .catch((error: unknown) => {
        if (!cancelled) {
          setState({ status: 'error', message: getApiErrorMessage(error), httpStatus: getApiErrorStatus(error) });
        }
      });
    return () => {
      cancelled = true;
    };
  }, [studentRepository, userRepository, reloadKey]);

  const childViews = useMemo(() => (state.status === 'ready' ? state.students.map(toView) : []), [state]);
  const currentChild = childViews.find((c) => c.id === selectedChildId) ?? childViews[0];

  let currentRole: ClassRole | null = null;
  if (state.status === 'ready' && currentChild?.classId) {
    currentRole = state.profile.classRoles.find((r) => r.classId === currentChild.classId)?.role ?? 'participant';
  }

  const value: ChildContextValue = {
    children: childViews,
    loading: state.status === 'loading',
    error: state.status === 'error' ? state.message : null,
    errorStatus: state.status === 'error' ? state.httpStatus : undefined,
    reload: () => setReloadKey((k) => k + 1),
    currentChildId: currentChild?.id ?? '',
    setCurrentChildId: setSelectedChildId,
    currentChild,
    currentSummary: currentChild ? EMPTY_SUMMARY : undefined,
    currentRole,
    otherGuardians: [],
    transferClassRepresentative: noop,
    assignClassRepresentative: noop,
    resignClassRepresentative: noop,
  };

  return <ChildContext.Provider value={value}>{children}</ChildContext.Provider>;
}

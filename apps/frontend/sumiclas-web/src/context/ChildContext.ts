import { createContext } from 'react';
import type { Child, DashboardSummary, OtherGuardian } from '@/data/appMock';
import type { ClassRole } from '@/lib/roles';

export type ChildContextValue = {
  children: Child[];
  currentChildId: string;
  setCurrentChildId: (id: string) => void;
  currentChild: Child | undefined;
  currentSummary: DashboardSummary | undefined;
  currentRole: ClassRole | null;
  otherGuardians: OtherGuardian[];
  transferClassRepresentative: (guardianId: string) => void;
  assignClassRepresentative: (guardianId: string) => void;
  resignClassRepresentative: () => void;
};

export const ChildContext = createContext<ChildContextValue | null>(null);

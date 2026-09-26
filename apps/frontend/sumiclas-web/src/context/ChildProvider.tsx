import { useState, type ReactNode } from 'react';
import {
  childrenMock,
  summaryByChild,
  classRoleByChild as classRoleByChildMock,
  otherGuardiansByChild as otherGuardiansByChildMock,
  type OtherGuardian,
} from '@/data/appMock';
import { ChildContext, type ChildContextValue } from './ChildContext';
import { canManageClassRepresentatives, MAX_CLASS_REPRESENTATIVES, type ClassRole } from '@/lib/roles';

export function ChildProvider({ children }: { children: ReactNode }) {
  const [currentChildId, setCurrentChildId] = useState(childrenMock[0]?.id ?? '');
  const [classRoleByChild, setClassRoleByChild] = useState<Record<string, ClassRole>>(classRoleByChildMock);
  const [otherGuardiansByChild, setOtherGuardiansByChild] = useState<Record<string, OtherGuardian[]>>(
    otherGuardiansByChildMock
  );

  const currentChild = childrenMock.find((c) => c.id === currentChildId);
  const currentRole = classRoleByChild[currentChildId] ?? null;
  const otherGuardians = otherGuardiansByChild[currentChildId] ?? [];

  const transferClassRepresentative = (guardianId: string) => {
    if (classRoleByChild[currentChildId] !== 'classRepresentative') return;
    setClassRoleByChild((prev) => ({ ...prev, [currentChildId]: 'participant' }));
    setOtherGuardiansByChild((prev) => ({
      ...prev,
      [currentChildId]: (prev[currentChildId] ?? []).map((g) =>
        g.id === guardianId ? { ...g, role: 'classRepresentative' } : g
      ),
    }));
  };

  const resignClassRepresentative = () => {
    const guardians = otherGuardiansByChild[currentChildId] ?? [];
    const otherRepresentatives = guardians.filter((g) => g.role === 'classRepresentative').length;
    if (classRoleByChild[currentChildId] !== 'classRepresentative' || otherRepresentatives < 1) return;
    setClassRoleByChild((prev) => ({ ...prev, [currentChildId]: 'participant' }));
  };

  const assignClassRepresentative = (guardianId: string) => {
    const guardians = otherGuardiansByChild[currentChildId] ?? [];
    const representativeCount =
      guardians.filter((g) => g.role === 'classRepresentative').length +
      (classRoleByChild[currentChildId] === 'classRepresentative' ? 1 : 0);
    if (
      !canManageClassRepresentatives(classRoleByChild[currentChildId] ?? null) ||
      representativeCount >= MAX_CLASS_REPRESENTATIVES
    )
      return;
    setOtherGuardiansByChild((prev) => ({
      ...prev,
      [currentChildId]: (prev[currentChildId] ?? []).map((g) =>
        g.id === guardianId ? { ...g, role: 'classRepresentative' } : g
      ),
    }));
  };

  const value: ChildContextValue = {
    children: childrenMock,
    currentChildId,
    setCurrentChildId,
    currentChild,
    currentSummary: currentChild ? summaryByChild[currentChild.id] : undefined,
    currentRole,
    otherGuardians,
    transferClassRepresentative,
    assignClassRepresentative,
    resignClassRepresentative,
  };

  return <ChildContext.Provider value={value}>{children}</ChildContext.Provider>;
}

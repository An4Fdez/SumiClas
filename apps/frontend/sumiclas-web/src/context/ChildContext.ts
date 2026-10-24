import { createContext } from 'react';
import type { ClassRole } from '@/lib/roles';
import type { Child } from '@/modules/students/domain/Child';

export const NO_CLASS_LABEL = 'Sin clase';

/** A child as the screens show it: the label fields are never empty. */
export type ChildView = Child & {
  className: string;
  schoolName: string;
};

export type ChildContextValue = {
  children: ChildView[];
  loading: boolean;
  error: string | null;
  errorStatus: number | undefined;
  reload: () => void;
  refresh: () => Promise<void>;
  currentChildId: string;
  setCurrentChildId: (id: string) => void;
  currentChild: ChildView | undefined;
  currentRole: ClassRole | null;
  currentUserId: string | undefined;
};

export const ChildContext = createContext<ChildContextValue | null>(null);

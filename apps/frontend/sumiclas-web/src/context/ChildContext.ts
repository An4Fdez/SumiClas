import { createContext } from 'react';
import type { HijoContexto, ResumenSecciones, OtroTutor } from '@/data/appMock';
import type { ClassRole } from '@/lib/roles';

export type ChildContextValue = {
  hijos: HijoContexto[];
  hijoActualId: string;
  setHijoActualId: (id: string) => void;
  hijoActual: HijoContexto | undefined;
  resumenActual: ResumenSecciones | undefined;
  rolActual: ClassRole | null;
  otrosTutoresActual: OtroTutor[];
  cederRolDelegado: (tutorId: string) => void;
  asignarDelegado: (tutorId: string) => void;
  renunciarDelegado: () => void;
};

export const ChildContext = createContext<ChildContextValue | null>(null);

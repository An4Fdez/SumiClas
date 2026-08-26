import { createContext } from 'react';
import type { HijoContexto, ResumenSecciones } from '@/data/appMock';

export type ChildContextValue = {
  hijos: HijoContexto[];
  hijoActualId: string;
  setHijoActualId: (id: string) => void;
  hijoActual: HijoContexto | undefined;
  resumenActual: ResumenSecciones | undefined;
};

export const ChildContext = createContext<ChildContextValue | null>(null);

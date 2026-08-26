import { useMemo, useState, type ReactNode } from 'react';
import { hijosMock, resumenPorHijo } from '@/data/appMock';
import { ChildContext, type ChildContextValue } from './ChildContext';

export function ChildProvider({ children }: { children: ReactNode }) {
  const [hijoActualId, setHijoActualId] = useState(hijosMock[0]?.id ?? '');

  const value = useMemo<ChildContextValue>(() => {
    const hijoActual = hijosMock.find((h) => h.id === hijoActualId);
    return {
      hijos: hijosMock,
      hijoActualId,
      setHijoActualId,
      hijoActual,
      resumenActual: hijoActual ? resumenPorHijo[hijoActual.id] : undefined,
    };
  }, [hijoActualId]);

  return <ChildContext.Provider value={value}>{children}</ChildContext.Provider>;
}

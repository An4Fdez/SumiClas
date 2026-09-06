import { useState, type ReactNode } from 'react';
import {
  hijosMock,
  resumenPorHijo,
  rolClasePorHijo as rolClasePorHijoMock,
  otrosTutoresPorHijo as otrosTutoresPorHijoMock,
  type OtroTutor,
} from '@/data/appMock';
import { ChildContext, type ChildContextValue } from './ChildContext';
import { canManageDelegados, MAX_DELEGADOS, type ClassRole } from '@/lib/roles';

export function ChildProvider({ children }: { children: ReactNode }) {
  const [hijoActualId, setHijoActualId] = useState(hijosMock[0]?.id ?? '');
  const [rolClasePorHijo, setRolClasePorHijo] = useState<Record<string, ClassRole>>(rolClasePorHijoMock);
  const [otrosTutoresPorHijo, setOtrosTutoresPorHijo] = useState<Record<string, OtroTutor[]>>(
    otrosTutoresPorHijoMock
  );

  const hijoActual = hijosMock.find((h) => h.id === hijoActualId);
  const rolActual = rolClasePorHijo[hijoActualId] ?? null;
  const otrosTutoresActual = otrosTutoresPorHijo[hijoActualId] ?? [];

  const cederRolDelegado = (tutorId: string) => {
    if (rolClasePorHijo[hijoActualId] !== 'delegado') return;
    setRolClasePorHijo((prev) => ({ ...prev, [hijoActualId]: 'participante' }));
    setOtrosTutoresPorHijo((prev) => ({
      ...prev,
      [hijoActualId]: (prev[hijoActualId] ?? []).map((t) => (t.id === tutorId ? { ...t, rol: 'delegado' } : t)),
    }));
  };

  const renunciarDelegado = () => {
    const tutores = otrosTutoresPorHijo[hijoActualId] ?? [];
    const otrosDelegados = tutores.filter((t) => t.rol === 'delegado').length;
    if (rolClasePorHijo[hijoActualId] !== 'delegado' || otrosDelegados < 1) return;
    setRolClasePorHijo((prev) => ({ ...prev, [hijoActualId]: 'participante' }));
  };

  const asignarDelegado = (tutorId: string) => {
    const tutores = otrosTutoresPorHijo[hijoActualId] ?? [];
    const numDelegados =
      tutores.filter((t) => t.rol === 'delegado').length + (rolClasePorHijo[hijoActualId] === 'delegado' ? 1 : 0);
    if (!canManageDelegados(rolClasePorHijo[hijoActualId] ?? null) || numDelegados >= MAX_DELEGADOS) return;
    setOtrosTutoresPorHijo((prev) => ({
      ...prev,
      [hijoActualId]: (prev[hijoActualId] ?? []).map((t) => (t.id === tutorId ? { ...t, rol: 'delegado' } : t)),
    }));
  };

  const value: ChildContextValue = {
    hijos: hijosMock,
    hijoActualId,
    setHijoActualId,
    hijoActual,
    resumenActual: hijoActual ? resumenPorHijo[hijoActual.id] : undefined,
    rolActual,
    otrosTutoresActual,
    cederRolDelegado,
    asignarDelegado,
    renunciarDelegado,
  };

  return <ChildContext.Provider value={value}>{children}</ChildContext.Provider>;
}

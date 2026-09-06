import type { ClassRole } from '@/lib/roles';

export type HijoContexto = {
  id: string;
  alumnoNombre: string;
  colegioNombre: string;
  claseNombre: string;
};

export type ResumenSecciones = {
  avisos: { nuevos: number; urgente: boolean };
  calendario: { proximo: string | null; enDias: number | null };
  deberes: { pendientes: number; proxima: string | null };
  gastos: { pendiente: boolean; concepto?: string };
  objetosPerdidos: { hayNovedades: boolean; cantidad?: number };
};

export const hijosMock: HijoContexto[] = [
  { id: 'h1', alumnoNombre: 'Martina R.', colegioNombre: 'CEIP Miguel de Cervantes', claseNombre: '3ºA' },
  { id: 'h2', alumnoNombre: 'Pablo R.', colegioNombre: 'CEIP Miguel de Cervantes', claseNombre: '1ºB' },
];

export type ResumenIA = {
  fecha: string;
  puntos: string[];
};

export const resumenIAPorHijo: Record<string, ResumenIA> = {
  h1: {
    fecha: '18 oct',
    puntos: [
      'Jueves: excursión al museo. Hay que entregar la autorización firmada.',
      'Viernes: examen de matemáticas (temas 3 y 4).',
      'Quedan 6 familias por pagar el regalo de la profe.',
    ],
  },
  h2: {
    fecha: '18 oct',
    puntos: ['Sin avisos nuevos esta semana.', 'El 25 de octubre hay reunión de padres a las 17:00.'],
  },
};

export const resumenPorHijo: Record<string, ResumenSecciones> = {
  h1: {
    avisos: { nuevos: 3, urgente: true },
    calendario: { proximo: 'Excursión al museo', enDias: 3 },
    deberes: { pendientes: 1, proxima: 'mañana' },
    gastos: { pendiente: true, concepto: 'Regalo de la profe' },
    objetosPerdidos: { hayNovedades: true, cantidad: 1 },
  },
  h2: {
    avisos: { nuevos: 0, urgente: false },
    calendario: { proximo: 'Reunión de padres', enDias: 7 },
    deberes: { pendientes: 0, proxima: null },
    gastos: { pendiente: false },
    objetosPerdidos: { hayNovedades: false },
  },
};

export type OtroTutor = {
  id: string;
  nombre: string;
  rol: ClassRole;
};

export const rolClasePorHijo: Record<string, ClassRole> = {
  h1: 'delegado',
  h2: 'participante',
};

export const otrosTutoresPorHijo: Record<string, OtroTutor[]> = {
  h1: [
    { id: 't1', nombre: 'Carlos Gómez', rol: 'participante' },
    { id: 't2', nombre: 'Laura Sáez', rol: 'participante' },
  ],
  h2: [{ id: 't3', nombre: 'Marta Ibáñez', rol: 'delegado' }],
};

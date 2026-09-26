import type { ClassRole } from '@/lib/roles';

export type Child = {
  id: string;
  studentName: string;
  schoolName: string;
  className: string;
};

export type DashboardSummary = {
  announcements: { new: number; urgent: boolean };
  calendar: { next: string | null; daysUntil: number | null };
  homework: { pending: number; nextDueDate: string | null };
  expenses: { pending: boolean; item?: string };
  lostAndFound: { hasUpdates: boolean; count?: number };
};

export const childrenMock: Child[] = [
  { id: 'h1', studentName: 'Martina R.', schoolName: 'CEIP Miguel de Cervantes', className: '3ºA' },
  { id: 'h2', studentName: 'Pablo R.', schoolName: 'CEIP Miguel de Cervantes', className: '1ºB' },
];

export type AISummary = {
  date: string;
  points: string[];
};

export const aiSummaryByChild: Record<string, AISummary> = {
  h1: {
    date: '18 oct',
    points: [
      'Jueves: excursión al museo. Hay que entregar la autorización firmada.',
      'Viernes: examen de matemáticas (temas 3 y 4).',
      'Quedan 6 familias por pagar el regalo de la profe.',
    ],
  },
  h2: {
    date: '18 oct',
    points: ['Sin avisos nuevos esta semana.', 'El 25 de octubre hay reunión de padres a las 17:00.'],
  },
};

export const summaryByChild: Record<string, DashboardSummary> = {
  h1: {
    announcements: { new: 3, urgent: true },
    calendar: { next: 'Excursión al museo', daysUntil: 3 },
    homework: { pending: 1, nextDueDate: 'mañana' },
    expenses: { pending: true, item: 'Regalo de la profe' },
    lostAndFound: { hasUpdates: true, count: 1 },
  },
  h2: {
    announcements: { new: 0, urgent: false },
    calendar: { next: 'Reunión de padres', daysUntil: 7 },
    homework: { pending: 0, nextDueDate: null },
    expenses: { pending: false },
    lostAndFound: { hasUpdates: false },
  },
};

export type OtherGuardian = {
  id: string;
  name: string;
  role: ClassRole;
};

export const classRoleByChild: Record<string, ClassRole> = {
  h1: 'classRepresentative',
  h2: 'participant',
};

export const otherGuardiansByChild: Record<string, OtherGuardian[]> = {
  h1: [
    { id: 't1', name: 'Carlos Gómez', role: 'participant' },
    { id: 't2', name: 'Laura Sáez', role: 'participant' },
  ],
  h2: [{ id: 't3', name: 'Marta Ibáñez', role: 'classRepresentative' }],
};

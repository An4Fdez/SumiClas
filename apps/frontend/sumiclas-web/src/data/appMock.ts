import type { ClassRole } from '@/lib/roles';

export type DashboardSummary = {
  announcements: { new: number; urgent: boolean };
  calendar: { next: string | null; daysUntil: number | null };
  homework: { pending: number; nextDueDate: string | null };
  expenses: { pending: boolean; item?: string };
  lostAndFound: { hasUpdates: boolean; count?: number };
};

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

export type OtherGuardian = {
  id: string;
  name: string;
  role: ClassRole;
};

export const otherGuardiansByChild: Record<string, OtherGuardian[]> = {
  h1: [
    { id: 't1', name: 'Carlos Gómez', role: 'participant' },
    { id: 't2', name: 'Laura Sáez', role: 'participant' },
  ],
  h2: [{ id: 't3', name: 'Marta Ibáñez', role: 'classRepresentative' }],
};

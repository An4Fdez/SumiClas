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

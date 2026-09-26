import { calendarEventsMock, type RelatedEvent } from '@/data/calendarMock';

export type { RelatedEvent };
export type Priority = 'low' | 'normal' | 'high';
export type AnnouncementScope = 'school' | 'class';

export type Announcement = {
  id: string;
  scope: AnnouncementScope;
  /** Only if scope === 'class': which child/class it belongs to. */
  childId?: string;
  title: string;
  body: string;
  priority: Priority;
  authorName: string;
  authorRole: string;
  date: string;
  relatedEvent?: RelatedEvent;
  read: boolean;
};

function relatedEventFor(id: string): RelatedEvent {
  const event = calendarEventsMock.find((ev) => ev.id === id);
  if (!event) throw new Error(`Evento de calendario no encontrado: ${id}`);
  return { id: event.id, title: event.title };
}

const now = Date.now();
const hoursAgo = (h: number) => new Date(now - h * 3_600_000).toISOString();
const daysAgo = (d: number) => new Date(now - d * 86_400_000).toISOString();

export const schoolAnnouncementsMock: Announcement[] = [
  {
    id: 'ac1',
    scope: 'school',
    title: 'Nuevo horario de comedor a partir de noviembre',
    body:
      'A partir del 1 de noviembre el comedor abre 10 minutos antes para facilitar la recogida. No afecta al precio del servicio.',
    priority: 'normal',
    authorName: 'Dirección',
    authorRole: 'Colegio',
    date: daysAgo(1),
    read: true,
  },
  {
    id: 'ac2',
    scope: 'school',
    title: 'Recogida de donativos para el mercadillo solidario',
    body:
      'El AMPA organiza el mercadillo solidario anual. Se aceptan juguetes y libros en buen estado hasta el 20 de octubre.',
    priority: 'low',
    authorName: 'AMPA',
    authorRole: 'AMPA',
    date: daysAgo(4),
    read: true,
  },
];

export const classAnnouncementsByChild: Record<string, Announcement[]> = {
  h1: [
    {
      id: 'c1-1',
      scope: 'class',
      childId: 'h1',
      title: 'Entregad la autorización de la excursión',
      body: 'Recordad entregar la autorización firmada antes del jueves para poder ir al museo.',
      priority: 'high',
      authorName: 'Elena Torres',
      authorRole: 'Profesora',
      date: hoursAgo(3),
      relatedEvent: relatedEventFor('ev1'),
      read: false,
    },
    {
      id: 'c1-2',
      scope: 'class',
      childId: 'h1',
      title: 'Examen de matemáticas el viernes',
      body: 'Entra materia de los temas 3 y 4. Pueden traer calculadora básica.',
      priority: 'normal',
      authorName: 'Elena Torres',
      authorRole: 'Profesora',
      date: hoursAgo(20),
      read: false,
    },
    {
      id: 'c1-3',
      scope: 'class',
      childId: 'h1',
      title: 'Fotos de la fiesta de otoño ya disponibles',
      body: 'Podéis pedirlas al delegado de clase si queréis una copia.',
      priority: 'low',
      authorName: 'Carlos Gómez',
      authorRole: 'Delegado de padres',
      date: daysAgo(2),
      read: true,
    },
  ],
  h2: [
    {
      id: 'c2-1',
      scope: 'class',
      childId: 'h2',
      title: 'Reunión de padres el 25 de octubre',
      body: 'Se convoca reunión de inicio de curso a las 17:00 en el aula.',
      priority: 'normal',
      authorName: 'Marta Ibáñez',
      authorRole: 'Delegada de padres',
      date: daysAgo(1),
      relatedEvent: relatedEventFor('ev2'),
      read: true,
    },
  ],
};

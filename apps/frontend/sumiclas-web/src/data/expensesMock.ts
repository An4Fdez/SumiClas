import { calendarEventsMock, type RelatedEvent } from '@/data/calendarMock';

export type ClassExpense = {
  id: string;
  /** Expenses always belong to a specific child's class (never "school"). */
  childId: string;
  item: string;
  /** Euros. */
  totalAmount: number;
  /** Ids of families/guardians in the class who participate: 'me' + otherGuardiansByChild ids. */
  participantIds: string[];
  /** Subset of participantIds who have already marked their own payment. */
  paidBy: string[];
  relatedEvent?: RelatedEvent;
  /** ISO, creation date. */
  date: string;
  authorName: string;
  authorRole: string;
};

function relatedEventFor(id: string): RelatedEvent {
  const event = calendarEventsMock.find((ev) => ev.id === id);
  if (!event) throw new Error(`Evento de calendario no encontrado: ${id}`);
  return { id: event.id, title: event.title };
}

const now = new Date();
const daysAgo = (d: number) => {
  const date = new Date(now);
  date.setDate(date.getDate() - d);
  return date.toISOString();
};

export const classExpensesMock: ClassExpense[] = [
  {
    id: 'gc1',
    childId: 'h1',
    item: 'Regalo de fin de curso para la profe',
    totalAmount: 30,
    participantIds: ['me', 't1', 't2'],
    paidBy: ['t1', 't2'],
    date: daysAgo(3),
    authorName: 'Tú',
    authorRole: 'Delegado/a de padres',
  },
  {
    id: 'gc2',
    childId: 'h1',
    item: 'Excursión al museo',
    totalAmount: 15,
    participantIds: ['me', 't1', 't2'],
    paidBy: ['me', 't1'],
    relatedEvent: relatedEventFor('ev1'),
    date: daysAgo(1),
    authorName: 'Tú',
    authorRole: 'Delegado/a de padres',
  },
  {
    id: 'gc3',
    childId: 'h2',
    item: 'Material de plástica para todo el curso',
    totalAmount: 20,
    participantIds: ['me', 't3'],
    paidBy: ['t3'],
    date: daysAgo(5),
    authorName: 'Marta Ibáñez',
    authorRole: 'Delegada de padres',
  },
];

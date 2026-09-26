import type { PaletteColor } from '@/lib/colorClasses';
import { normalizeText } from '@/lib/text';

/** Subjects aren't a closed catalog: a class representative/teacher can create a new one when creating an assignment. */
export type SubjectInfo = {
  id: string;
  label: string;
  color: PaletteColor;
};

export type Assignment = {
  id: string;
  /** Homework always belongs to a specific child's class (never "school"). */
  childId: string;
  subjectId: string;
  title: string;
  description?: string;
  /** ISO, due date. */
  dueDate: string;
  authorName: string;
  authorRole: string;
  /** true if created by a participant (not an official one from the teacher/class representative). */
  isProposal: boolean;
};

export type AssignmentComment = {
  id: string;
  assignmentId: string;
  authorName: string;
  authorRole: string;
  text: string;
  date: string;
};

export const availableSubjectsMock: SubjectInfo[] = [
  { id: 'matematicas', label: 'Matemáticas', color: 'brand' },
  { id: 'lengua', label: 'Lengua', color: 'rose' },
  { id: 'ingles', label: 'Inglés', color: 'violet' },
  { id: 'naturales', label: 'Naturales', color: 'mint' },
  { id: 'sociales', label: 'Sociales', color: 'sun' },
  { id: 'plastica', label: 'Plástica', color: 'orange' },
];

const cycleColors: PaletteColor[] = ['brand', 'rose', 'violet', 'mint', 'sun', 'orange'];

/** Looks up a subject by id in the given list; if missing (inconsistent data), falls back to a neutral value. */
export function findSubject(id: string, available: SubjectInfo[]): SubjectInfo {
  return available.find((a) => a.id === id) ?? { id, label: id, color: 'orange' };
}

/** Creates (or reuses if one with the same name already exists) a new subject from free text. */
export function createSubject(name: string, available: SubjectInfo[]): SubjectInfo {
  const id = normalizeText(name).replace(/\s+/g, '-');
  const existing = available.find((a) => a.id === id);
  if (existing) return existing;
  return { id, label: name.trim(), color: cycleColors[available.length % cycleColors.length] };
}

const now = new Date();
const inDays = (d: number) => {
  const date = new Date(now);
  date.setDate(date.getDate() + d);
  date.setHours(23, 59, 0, 0);
  return date.toISOString();
};
const hoursAgo = (h: number) => new Date(now.getTime() - h * 3_600_000).toISOString();

export const assignmentsMock: Assignment[] = [
  {
    id: 'td1',
    childId: 'h1',
    subjectId: 'matematicas',
    title: 'Ejercicios página 45',
    description: 'Del 1 al 8, todos con las operaciones desarrolladas.',
    dueDate: inDays(1),
    authorName: 'Elena Torres',
    authorRole: 'Profesora',
    isProposal: false,
  },
  {
    id: 'td2',
    childId: 'h1',
    subjectId: 'lengua',
    title: 'Lectura capítulo 3',
    description: 'Leer el capítulo 3 de "El principito" y responder a las preguntas de la ficha.',
    dueDate: inDays(4),
    authorName: 'Elena Torres',
    authorRole: 'Profesora',
    isProposal: false,
  },
  {
    id: 'td3',
    childId: 'h1',
    subjectId: 'plastica',
    title: 'Traer material de plástica',
    description: 'Cartulina de color y tijeras de punta redonda.',
    dueDate: inDays(2),
    authorName: 'Carlos Gómez',
    authorRole: 'Familia',
    isProposal: true,
  },
  {
    id: 'td4',
    childId: 'h2',
    subjectId: 'naturales',
    title: 'Ficha del ciclo del agua',
    dueDate: inDays(3),
    authorName: 'Marta Ibáñez',
    authorRole: 'Delegada de padres',
    isProposal: false,
  },
];

export const assignmentCommentsMock: AssignmentComment[] = [
  {
    id: 'cd1',
    assignmentId: 'td1',
    authorName: 'Laura Sáez',
    authorRole: 'Familia',
    text: '¿También el ejercicio 8 o solo hasta el 7?',
    date: hoursAgo(5),
  },
  {
    id: 'cd2',
    assignmentId: 'td1',
    authorName: 'Elena Torres',
    authorRole: 'Profesora',
    text: 'Todos, del 1 al 8 incluido.',
    date: hoursAgo(4),
  },
];

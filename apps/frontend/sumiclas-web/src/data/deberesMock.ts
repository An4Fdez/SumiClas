import type { PaletteColor } from '@/lib/colorClasses';
import { normalizeText } from '@/lib/text';

/** Las asignaturas no son un catálogo cerrado: delegado/profesor puede crear una nueva al crear una tarea. */
export type AsignaturaInfo = {
  id: string;
  label: string;
  color: PaletteColor;
};

export type TareaDeberes = {
  id: string;
  /** Los deberes son siempre de la clase de un hijo concreto (nunca "colegio"). */
  hijoId: string;
  asignaturaId: string;
  titulo: string;
  descripcion?: string;
  /** ISO, fecha de entrega. */
  fecha: string;
  autorNombre: string;
  autorRol: string;
  /** true si la creó un participante (no oficial del profesor/delegado). */
  esPropuesta: boolean;
};

export type ComentarioDeberes = {
  id: string;
  tareaId: string;
  autorNombre: string;
  autorRol: string;
  texto: string;
  fecha: string;
};

export const asignaturasDisponiblesMock: AsignaturaInfo[] = [
  { id: 'matematicas', label: 'Matemáticas', color: 'brand' },
  { id: 'lengua', label: 'Lengua', color: 'rose' },
  { id: 'ingles', label: 'Inglés', color: 'violet' },
  { id: 'naturales', label: 'Naturales', color: 'mint' },
  { id: 'sociales', label: 'Sociales', color: 'sun' },
  { id: 'plastica', label: 'Plástica', color: 'orange' },
];

const cicloColores: PaletteColor[] = ['brand', 'rose', 'violet', 'mint', 'sun', 'orange'];

/** Busca una asignatura por id en la lista dada; si no existe (dato inconsistente), cae a un valor neutro. */
export function buscarAsignatura(id: string, disponibles: AsignaturaInfo[]): AsignaturaInfo {
  return disponibles.find((a) => a.id === id) ?? { id, label: id, color: 'orange' };
}

/** Crea (o reutiliza si ya existe con el mismo nombre) una asignatura nueva a partir de texto libre. */
export function crearAsignatura(nombre: string, disponibles: AsignaturaInfo[]): AsignaturaInfo {
  const id = normalizeText(nombre).replace(/\s+/g, '-');
  const existente = disponibles.find((a) => a.id === id);
  if (existente) return existente;
  return { id, label: nombre.trim(), color: cicloColores[disponibles.length % cicloColores.length] };
}

const now = new Date();
const inDays = (d: number) => {
  const fecha = new Date(now);
  fecha.setDate(fecha.getDate() + d);
  fecha.setHours(23, 59, 0, 0);
  return fecha.toISOString();
};
const hoursAgo = (h: number) => new Date(now.getTime() - h * 3_600_000).toISOString();

export const tareasDeberesMock: TareaDeberes[] = [
  {
    id: 'td1',
    hijoId: 'h1',
    asignaturaId: 'matematicas',
    titulo: 'Ejercicios página 45',
    descripcion: 'Del 1 al 8, todos con las operaciones desarrolladas.',
    fecha: inDays(1),
    autorNombre: 'Elena Torres',
    autorRol: 'Profesora',
    esPropuesta: false,
  },
  {
    id: 'td2',
    hijoId: 'h1',
    asignaturaId: 'lengua',
    titulo: 'Lectura capítulo 3',
    descripcion: 'Leer el capítulo 3 de "El principito" y responder a las preguntas de la ficha.',
    fecha: inDays(4),
    autorNombre: 'Elena Torres',
    autorRol: 'Profesora',
    esPropuesta: false,
  },
  {
    id: 'td3',
    hijoId: 'h1',
    asignaturaId: 'plastica',
    titulo: 'Traer material de plástica',
    descripcion: 'Cartulina de color y tijeras de punta redonda.',
    fecha: inDays(2),
    autorNombre: 'Carlos Gómez',
    autorRol: 'Familia',
    esPropuesta: true,
  },
  {
    id: 'td4',
    hijoId: 'h2',
    asignaturaId: 'naturales',
    titulo: 'Ficha del ciclo del agua',
    fecha: inDays(3),
    autorNombre: 'Marta Ibáñez',
    autorRol: 'Delegada de padres',
    esPropuesta: false,
  },
];

export const comentariosDeberesMock: ComentarioDeberes[] = [
  {
    id: 'cd1',
    tareaId: 'td1',
    autorNombre: 'Laura Sáez',
    autorRol: 'Familia',
    texto: '¿También el ejercicio 8 o solo hasta el 7?',
    fecha: hoursAgo(5),
  },
  {
    id: 'cd2',
    tareaId: 'td1',
    autorNombre: 'Elena Torres',
    autorRol: 'Profesora',
    texto: 'Todos, del 1 al 8 incluido.',
    fecha: hoursAgo(4),
  },
];

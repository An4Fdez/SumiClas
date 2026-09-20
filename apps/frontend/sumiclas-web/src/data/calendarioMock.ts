import { Bus, Cake, MoreHorizontal, Star, Users, type LucideIcon } from 'lucide-react';
import type { PaletteColor } from '@/lib/colorClasses';

export type EventoTipo = 'excursion' | 'reunion' | 'actividad' | 'cumpleanos' | 'otros';
export type EventoAlcance = 'colegio' | 'clase';

export type EventoCalendario = {
  id: string;
  alcance: EventoAlcance;
  /** Solo si alcance === 'clase': a qué hijo/clase pertenece. */
  hijoId?: string;
  tipo: EventoTipo;
  titulo: string;
  /**
   * ISO. Para tipo 'cumpleanos' se usa un año centinela (1900): estructuralmente
   * imposible derivar la edad real del menor. El emparejamiento con la cuadrícula
   * del calendario para estos eventos se hace solo por mes+día (ver lib/date.ts).
   */
  fecha: string;
  hora?: string;
  ubicacion?: string;
  /** Texto informativo, nunca estado de pago (eso vive en Gastos de clase). */
  coste?: string;
  autorNombre: string;
  autorRol: string;
};

export type EventoRelacionado = Pick<EventoCalendario, 'id' | 'titulo'>;

export const tipoEventoMeta: Record<EventoTipo, { label: string; color: PaletteColor; icon: LucideIcon }> = {
  excursion: { label: 'Excursión', color: 'mint', icon: Bus },
  reunion: { label: 'Reunión', color: 'violet', icon: Users },
  actividad: { label: 'Actividad', color: 'sun', icon: Star },
  cumpleanos: { label: 'Cumpleaños', color: 'rose', icon: Cake },
  otros: { label: 'Otros', color: 'orange', icon: MoreHorizontal },
};

const now = new Date();
const inDays = (d: number, hora?: { h: number; m: number }) => {
  const fecha = new Date(now);
  fecha.setDate(fecha.getDate() + d);
  if (hora) fecha.setHours(hora.h, hora.m, 0, 0);
  return fecha.toISOString();
};

export const eventosCalendarioMock: EventoCalendario[] = [
  {
    id: 'ev1',
    alcance: 'clase',
    hijoId: 'h1',
    tipo: 'excursion',
    titulo: 'Excursión al museo',
    fecha: inDays(3, { h: 9, m: 30 }),
    hora: '09:30',
    ubicacion: 'Museo de la Ciudad',
    coste: '5 € por alumno',
    autorNombre: 'Elena Torres',
    autorRol: 'Profesora',
  },
  {
    id: 'ev2',
    alcance: 'clase',
    hijoId: 'h2',
    tipo: 'reunion',
    titulo: 'Reunión de padres',
    fecha: inDays(7, { h: 17, m: 0 }),
    hora: '17:00',
    ubicacion: 'Aula 1ºB',
    autorNombre: 'Marta Ibáñez',
    autorRol: 'Delegada de padres',
  },
  {
    id: 'ev3',
    alcance: 'colegio',
    tipo: 'actividad',
    titulo: 'Mercadillo solidario del AMPA',
    fecha: inDays(10, { h: 10, m: 0 }),
    hora: '10:00',
    ubicacion: 'Patio del colegio',
    autorNombre: 'AMPA',
    autorRol: 'AMPA',
  },
  {
    id: 'ev-cumple-1',
    alcance: 'clase',
    hijoId: 'h1',
    tipo: 'cumpleanos',
    titulo: 'Cumpleaños de Martina',
    fecha: new Date(1900, 10, 5).toISOString(),
    autorNombre: 'Laura Sáez',
    autorRol: 'Familia',
  },
  {
    id: 'ev-cumple-2',
    alcance: 'clase',
    hijoId: 'h2',
    tipo: 'cumpleanos',
    titulo: 'Cumpleaños de Pablo',
    fecha: new Date(1900, 2, 12).toISOString(),
    autorNombre: 'Marta Ibáñez',
    autorRol: 'Familia',
  },
];

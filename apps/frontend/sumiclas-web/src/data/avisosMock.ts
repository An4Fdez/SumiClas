import { eventosCalendarioMock, type EventoRelacionado } from '@/data/calendarioMock';

export type { EventoRelacionado };
export type AvisoPrioridad = 'baja' | 'normal' | 'alta';
export type AvisoAlcance = 'colegio' | 'clase';

export type Aviso = {
  id: string;
  alcance: AvisoAlcance;
  /** Solo si alcance === 'clase': a qué hijo/clase pertenece. */
  hijoId?: string;
  titulo: string;
  cuerpo: string;
  prioridad: AvisoPrioridad;
  autorNombre: string;
  autorRol: string;
  fecha: string;
  eventoRelacionado?: EventoRelacionado;
  leido: boolean;
};

function eventoRelacionado(id: string): EventoRelacionado {
  const evento = eventosCalendarioMock.find((ev) => ev.id === id);
  if (!evento) throw new Error(`Evento de calendario no encontrado: ${id}`);
  return { id: evento.id, titulo: evento.titulo };
}

const now = Date.now();
const hoursAgo = (h: number) => new Date(now - h * 3_600_000).toISOString();
const daysAgo = (d: number) => new Date(now - d * 86_400_000).toISOString();

export const avisosColegioMock: Aviso[] = [
  {
    id: 'ac1',
    alcance: 'colegio',
    titulo: 'Nuevo horario de comedor a partir de noviembre',
    cuerpo:
      'A partir del 1 de noviembre el comedor abre 10 minutos antes para facilitar la recogida. No afecta al precio del servicio.',
    prioridad: 'normal',
    autorNombre: 'Dirección',
    autorRol: 'Colegio',
    fecha: daysAgo(1),
    leido: true,
  },
  {
    id: 'ac2',
    alcance: 'colegio',
    titulo: 'Recogida de donativos para el mercadillo solidario',
    cuerpo:
      'El AMPA organiza el mercadillo solidario anual. Se aceptan juguetes y libros en buen estado hasta el 20 de octubre.',
    prioridad: 'baja',
    autorNombre: 'AMPA',
    autorRol: 'AMPA',
    fecha: daysAgo(4),
    leido: true,
  },
];

export const avisosClasePorHijo: Record<string, Aviso[]> = {
  h1: [
    {
      id: 'c1-1',
      alcance: 'clase',
      hijoId: 'h1',
      titulo: 'Entregad la autorización de la excursión',
      cuerpo: 'Recordad entregar la autorización firmada antes del jueves para poder ir al museo.',
      prioridad: 'alta',
      autorNombre: 'Elena Torres',
      autorRol: 'Profesora',
      fecha: hoursAgo(3),
      eventoRelacionado: eventoRelacionado('ev1'),
      leido: false,
    },
    {
      id: 'c1-2',
      alcance: 'clase',
      hijoId: 'h1',
      titulo: 'Examen de matemáticas el viernes',
      cuerpo: 'Entra materia de los temas 3 y 4. Pueden traer calculadora básica.',
      prioridad: 'normal',
      autorNombre: 'Elena Torres',
      autorRol: 'Profesora',
      fecha: hoursAgo(20),
      leido: false,
    },
    {
      id: 'c1-3',
      alcance: 'clase',
      hijoId: 'h1',
      titulo: 'Fotos de la fiesta de otoño ya disponibles',
      cuerpo: 'Podéis pedirlas al delegado de clase si queréis una copia.',
      prioridad: 'baja',
      autorNombre: 'Carlos Gómez',
      autorRol: 'Delegado de padres',
      fecha: daysAgo(2),
      leido: true,
    },
  ],
  h2: [
    {
      id: 'c2-1',
      alcance: 'clase',
      hijoId: 'h2',
      titulo: 'Reunión de padres el 25 de octubre',
      cuerpo: 'Se convoca reunión de inicio de curso a las 17:00 en el aula.',
      prioridad: 'normal',
      autorNombre: 'Marta Ibáñez',
      autorRol: 'Delegada de padres',
      fecha: daysAgo(1),
      eventoRelacionado: eventoRelacionado('ev2'),
      leido: true,
    },
  ],
};

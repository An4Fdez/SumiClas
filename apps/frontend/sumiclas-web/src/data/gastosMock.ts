import { eventosCalendarioMock, type EventoRelacionado } from '@/data/calendarioMock';

export type GastoClase = {
  id: string;
  /** Los gastos son siempre de la clase de un hijo concreto (nunca "colegio"). */
  hijoId: string;
  concepto: string;
  /** Euros. */
  importeTotal: number;
  /** Ids de familias/tutores de la clase que participan: 'yo' + ids de otrosTutoresPorHijo. */
  participantesIds: string[];
  /** Subconjunto de participantesIds que ya han marcado su propio pago. */
  pagadoPor: string[];
  eventoRelacionado?: EventoRelacionado;
  /** ISO, fecha de creación. */
  fecha: string;
  autorNombre: string;
  autorRol: string;
};

function eventoRelacionado(id: string): EventoRelacionado {
  const evento = eventosCalendarioMock.find((ev) => ev.id === id);
  if (!evento) throw new Error(`Evento de calendario no encontrado: ${id}`);
  return { id: evento.id, titulo: evento.titulo };
}

const now = new Date();
const daysAgo = (d: number) => {
  const fecha = new Date(now);
  fecha.setDate(fecha.getDate() - d);
  return fecha.toISOString();
};

export const gastosClaseMock: GastoClase[] = [
  {
    id: 'gc1',
    hijoId: 'h1',
    concepto: 'Regalo de fin de curso para la profe',
    importeTotal: 30,
    participantesIds: ['yo', 't1', 't2'],
    pagadoPor: ['t1', 't2'],
    fecha: daysAgo(3),
    autorNombre: 'Tú',
    autorRol: 'Delegado/a de padres',
  },
  {
    id: 'gc2',
    hijoId: 'h1',
    concepto: 'Excursión al museo',
    importeTotal: 15,
    participantesIds: ['yo', 't1', 't2'],
    pagadoPor: ['yo', 't1'],
    eventoRelacionado: eventoRelacionado('ev1'),
    fecha: daysAgo(1),
    autorNombre: 'Tú',
    autorRol: 'Delegado/a de padres',
  },
  {
    id: 'gc3',
    hijoId: 'h2',
    concepto: 'Material de plástica para todo el curso',
    importeTotal: 20,
    participantesIds: ['yo', 't3'],
    pagadoPor: ['t3'],
    fecha: daysAgo(5),
    autorNombre: 'Marta Ibáñez',
    autorRol: 'Delegada de padres',
  },
];

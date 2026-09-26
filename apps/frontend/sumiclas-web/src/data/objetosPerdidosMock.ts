import { PackageCheck, Search, type LucideIcon } from 'lucide-react';
import type { PaletteColor } from '@/lib/colorClasses';

export type ObjetoTipo = 'buscado' | 'encontrado';
export type ObjetoAlcance = 'colegio' | 'clase';

export type ObjetoPerdido = {
  id: string;
  alcance: ObjetoAlcance;
  /** Solo si alcance === 'clase'. */
  hijoId?: string;
  /** "Busco" vs "He encontrado" — no cambia tras crearlo. */
  tipo: ObjetoTipo;
  /** Estado de resolución, independiente del tipo. */
  recuperado: boolean;
  descripcion: string;
  /** Object URL local (URL.createObjectURL); no persiste entre sesiones. */
  fotoUrl?: string;
  fecha: string;
  autorNombre: string;
  autorRol: string;
};

export const tipoObjetoMeta: Record<ObjetoTipo, { label: string; color: PaletteColor; icon: LucideIcon }> = {
  buscado: { label: 'Busco', color: 'rose', icon: Search },
  encontrado: { label: 'Encontrado', color: 'mint', icon: PackageCheck },
};

const now = new Date();
const daysAgo = (d: number) => {
  const fecha = new Date(now);
  fecha.setDate(fecha.getDate() - d);
  return fecha.toISOString();
};

export const objetosPerdidosMock: ObjetoPerdido[] = [
  {
    id: 'op1',
    alcance: 'clase',
    hijoId: 'h1',
    tipo: 'buscado',
    recuperado: false,
    descripcion: 'Chaqueta azul marino talla 8, con el nombre puesto por dentro del cuello.',
    fecha: daysAgo(1),
    autorNombre: 'Laura Sáez',
    autorRol: 'Familia',
  },
  {
    id: 'op2',
    alcance: 'colegio',
    tipo: 'encontrado',
    recuperado: false,
    descripcion: 'Botella de agua verde encontrada en el patio, cerca de las pistas.',
    fecha: daysAgo(2),
    autorNombre: 'Carlos Gómez',
    autorRol: 'Familia',
  },
  {
    id: 'op3',
    alcance: 'clase',
    hijoId: 'h2',
    tipo: 'buscado',
    recuperado: true,
    descripcion: 'Estuche de lápices de colores, ya apareció en el fondo de la mochila.',
    fecha: daysAgo(6),
    autorNombre: 'Marta Ibáñez',
    autorRol: 'Delegada de padres',
  },
];

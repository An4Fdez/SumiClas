import { PackageCheck, Search, type LucideIcon } from 'lucide-react';
import type { PaletteColor } from '@/lib/colorClasses';

export type ItemStatus = 'lost' | 'found';
export type ItemScope = 'school' | 'class';

export type LostItem = {
  id: string;
  scope: ItemScope;
  /** Only if scope === 'class'. */
  childId?: string;
  /** "Busco" vs "He encontrado" (Spanish UI copy) — doesn't change after creation. */
  status: ItemStatus;
  /** Resolution state, independent of status. */
  recovered: boolean;
  description: string;
  /** Object URL local (URL.createObjectURL); no persiste entre sesiones. */
  photoUrl?: string;
  date: string;
  authorName: string;
  authorRole: string;
};

export const itemStatusMeta: Record<ItemStatus, { label: string; color: PaletteColor; icon: LucideIcon }> = {
  lost: { label: 'Busco', color: 'rose', icon: Search },
  found: { label: 'Encontrado', color: 'mint', icon: PackageCheck },
};

const now = new Date();
const daysAgo = (d: number) => {
  const date = new Date(now);
  date.setDate(date.getDate() - d);
  return date.toISOString();
};

export const lostAndFoundMock: LostItem[] = [
  {
    id: 'op1',
    scope: 'class',
    childId: 'h1',
    status: 'lost',
    recovered: false,
    description: 'Chaqueta azul marino talla 8, con el nombre puesto por dentro del cuello.',
    date: daysAgo(1),
    authorName: 'Laura Sáez',
    authorRole: 'Familia',
  },
  {
    id: 'op2',
    scope: 'school',
    status: 'found',
    recovered: false,
    description: 'Botella de agua verde encontrada en el patio, cerca de las pistas.',
    date: daysAgo(2),
    authorName: 'Carlos Gómez',
    authorRole: 'Familia',
  },
  {
    id: 'op3',
    scope: 'class',
    childId: 'h2',
    status: 'lost',
    recovered: true,
    description: 'Estuche de lápices de colores, ya apareció en el fondo de la mochila.',
    date: daysAgo(6),
    authorName: 'Marta Ibáñez',
    authorRole: 'Delegada de padres',
  },
];

import type { PaletteColor } from '@/lib/colorClasses';

export type ClassRole = 'delegado' | 'participante';

export const MAX_DELEGADOS = 3;

export const roleMeta: Record<ClassRole, { label: string; description: string; color: PaletteColor }> = {
  delegado: {
    label: 'Delegado/a de padres',
    description: 'Puede publicar avisos y eventos oficiales de la clase.',
    color: 'sun',
  },
  participante: {
    label: 'Participante',
    description: 'Puede comentar en Deberes y publicar en Objetos perdidos, pero no publicar avisos oficiales.',
    color: 'mint',
  },
};

export function canManageDelegados(rol: ClassRole | null): boolean {
  return rol === 'delegado';
}

import type { PaletteColor } from '@/lib/colorClasses';

export type ClassRole = 'classRepresentative' | 'participant';

export const MAX_CLASS_REPRESENTATIVES = 3;

export const roleMeta: Record<ClassRole, { label: string; description: string; color: PaletteColor }> = {
  classRepresentative: {
    label: 'Delegado/a de padres',
    description: 'Puede publicar avisos y eventos oficiales de la clase.',
    color: 'sun',
  },
  participant: {
    label: 'Participante',
    description: 'Puede comentar en Deberes y publicar en Objetos perdidos, pero no publicar avisos oficiales.',
    color: 'mint',
  },
};

export function roleLabel(role: ClassRole | undefined): string | undefined {
  return role ? roleMeta[role].label : undefined;
}

/** Maps the API role name to the UI union; an unknown or missing role stays `undefined` instead of being guessed. */
export function parseClassRole(value: string | null | undefined): ClassRole | undefined {
  const normalized = value?.toLowerCase();
  if (normalized === 'classrepresentative') return 'classRepresentative';
  if (normalized === 'participant') return 'participant';
  return undefined;
}

export function canManageClassRepresentatives(role: ClassRole | null): boolean {
  return role === 'classRepresentative';
}

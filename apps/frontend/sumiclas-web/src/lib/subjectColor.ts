import type { PaletteColor } from '@/lib/colorClasses';

const PALETTE: PaletteColor[] = ['brand', 'rose', 'violet', 'mint', 'sun', 'orange'];

/** The API has no colour for a subject: derive a stable one from its id so it never depends on list order. */
export function subjectColor(subjectId: string): PaletteColor {
  let hash = 0;
  for (let i = 0; i < subjectId.length; i++) hash = (hash * 31 + subjectId.charCodeAt(i)) >>> 0;
  return PALETTE[hash % PALETTE.length];
}

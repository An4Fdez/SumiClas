import type { ReactNode } from 'react';
import { LOGO_GRADIENT_CLASS, LOGO_TILE_INNER_CLASS } from '@/lib/colorClasses';

const SIZE_CLASSES = {
  9: 'h-9 w-9',
  10: 'h-10 w-10',
};

/** Rounded-square icon tile with the logo's ring: the brand look for icons that are not tied to one screen color. */
export function LogoTile({ children, size = 9 }: { children: ReactNode; size?: keyof typeof SIZE_CLASSES }) {
  return (
    <div className={`${SIZE_CLASSES[size]} shrink-0 rounded-xl p-0.5 ${LOGO_GRADIENT_CLASS}`}>
      <div className={`${LOGO_TILE_INNER_CLASS} rounded-[10px]`}>{children}</div>
    </div>
  );
}

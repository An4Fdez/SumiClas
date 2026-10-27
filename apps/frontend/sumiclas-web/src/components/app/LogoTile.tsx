import type { ReactNode } from 'react';
import { LOGO_GRADIENT_CLASS } from '@/lib/colorClasses';

/** Rounded-square icon tile with the logo's ring: the brand look for icons that are not tied to one screen color. */
export function LogoTile({ children }: { children: ReactNode }) {
  return (
    <div className={`h-9 w-9 shrink-0 rounded-xl p-0.5 ${LOGO_GRADIENT_CLASS}`}>
      <div className="grid h-full w-full place-items-center rounded-[10px] bg-violet-100 text-[#6464d0] dark:bg-violet-950 dark:text-[#9c9cf0]">
        {children}
      </div>
    </div>
  );
}

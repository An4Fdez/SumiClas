export type PaletteColor = 'rose' | 'brand' | 'mint' | 'sun' | 'violet' | 'orange';

export const badgeColorClasses: Record<PaletteColor, { bg: string; text: string }> = {
  rose: { bg: 'bg-rose-100 dark:bg-rose-900/40', text: 'text-rose-600 dark:text-rose-300' },
  brand: { bg: 'bg-brand-100 dark:bg-brand-900/40', text: 'text-brand-600 dark:text-brand-300' },
  mint: { bg: 'bg-mint-100 dark:bg-mint-900/40', text: 'text-mint-600 dark:text-mint-300' },
  sun: { bg: 'bg-sun-100 dark:bg-sun-900/40', text: 'text-sun-600 dark:text-sun-300' },
  violet: { bg: 'bg-violet-100 dark:bg-violet-900/40', text: 'text-violet-600 dark:text-violet-300' },
  orange: { bg: 'bg-orange-100 dark:bg-orange-900/40', text: 'text-orange-600 dark:text-orange-300' },
};

/** The stroke colors of the logo, in its order; used as a thin ring around the tiles that carry the brand (child avatar, AI summary). */
export const LOGO_GRADIENT_CLASS = 'bg-[linear-gradient(135deg,#00f090,#00d8f0,#7878d8,#f03078,#ffa830)]';

/** The inner face of the logo-ringed tiles (the square `LogoTile` and the round buttons); the caller adds the corner rounding. */
export const LOGO_TILE_INNER_CLASS =
  'grid h-full w-full place-items-center bg-violet-100 text-[#6464d0] dark:bg-violet-950 dark:text-[#9c9cf0]';

export const dotColorClasses: Record<PaletteColor, string> = {
  rose: 'bg-rose-500',
  brand: 'bg-brand-500',
  mint: 'bg-mint-500',
  sun: 'bg-sun-500',
  violet: 'bg-violet-500',
  orange: 'bg-orange-500',
};

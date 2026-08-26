export type PaletteColor = 'rose' | 'brand' | 'mint' | 'sun' | 'violet' | 'orange';

export const badgeColorClasses: Record<PaletteColor, { bg: string; text: string }> = {
  rose: { bg: 'bg-rose-100 dark:bg-rose-900/40', text: 'text-rose-600 dark:text-rose-300' },
  brand: { bg: 'bg-brand-100 dark:bg-brand-900/40', text: 'text-brand-600 dark:text-brand-300' },
  mint: { bg: 'bg-mint-100 dark:bg-mint-900/40', text: 'text-mint-600 dark:text-mint-300' },
  sun: { bg: 'bg-sun-100 dark:bg-sun-900/40', text: 'text-sun-600 dark:text-sun-300' },
  violet: { bg: 'bg-violet-100 dark:bg-violet-900/40', text: 'text-violet-600 dark:text-violet-300' },
  orange: { bg: 'bg-orange-100 dark:bg-orange-900/40', text: 'text-orange-600 dark:text-orange-300' },
};

/**
 * One entry per letter of «Clas», following the order in which the logo strokes flow (green, violet, pink, orange).
 * Light mode uses deeper shades of each hue so the italic letters keep about 3:1 against white and the footer's grey;
 * dark mode uses the logo hues as they are. The classes are written out whole so Tailwind can find them.
 */
export const WORDMARK_LETTER_COLORS = [
  'text-[#009a60] dark:text-[#00f090]',
  'text-[#6a6acf] dark:text-[#7878d8]',
  'text-[#e8266f] dark:text-[#f03078]',
  'text-[#d27200] dark:text-[#ffa830]',
] as const;

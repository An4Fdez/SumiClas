export function normalizeText(value: string): string {
  return value.trim().toLowerCase().normalize('NFD').replace(/\p{Diacritic}/gu, '');
}

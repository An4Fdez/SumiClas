const CENTS_PER_EURO = 100;

const euros = new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' });

/** The backend column is decimal(10,2): 99,999,999.99 € in cents. */
export const MAX_TOTAL_CENTS = 9_999_999_999;

export function formatEuros(amount: number): string {
  return euros.format(amount);
}

export type AmountParse = { cents: number } | { error: string };

/** Parses what the user typed (`5`, `5,5`, `5.50`) into whole cents so nothing is ever computed with floats. */
export function parseEuroAmount(input: string): AmountParse {
  const text = input.trim().replace(',', '.');
  if (!/^\d+(\.\d*)?$/.test(text)) return { error: 'Escribe un importe válido, por ejemplo 5 o 5,50.' };
  const [whole, fraction = ''] = text.split('.');
  if (fraction.length > 2) return { error: 'El importe no puede tener más de 2 decimales.' };
  const cents = Number(whole) * CENTS_PER_EURO +Number(fraction.padEnd(2, '0'));
  if (!Number.isSafeInteger(cents) || cents <= 0) return { error: 'El importe debe ser mayor que cero.' };
  return { cents };
}

export function centsToEuros(cents: number): number {
  return cents / CENTS_PER_EURO;
}

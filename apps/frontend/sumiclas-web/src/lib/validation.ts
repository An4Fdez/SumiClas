const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;

export function isValidEmail(email: string): boolean {
  return EMAIL_REGEX.test(email.trim());
}

export const MIN_PASSWORD_LENGTH = 8;
export const RECOMMENDED_PASSWORD_LENGTH = 12;

async function sha1Hex(value: string): Promise<string> {
  const buffer = await crypto.subtle.digest('SHA-1', new TextEncoder().encode(value));
  return Array.from(new Uint8Array(buffer))
    .map((b) => b.toString(16).padStart(2, '0'))
    .join('')
    .toUpperCase();
}

/**
 * Comprueba la contraseña contra la API pública "Have I Been Pwned" (Pwned Passwords)
 * usando k-anonimato: solo se envían los 5 primeros caracteres del hash SHA-1, nunca
 * la contraseña real ni el hash completo. Si la API no responde, no bloqueamos el
 * registro (fail-open) — es una comprobación adicional, no la única barrera.
 */
export async function isPasswordPwned(password: string): Promise<boolean> {
  try {
    const hash = await sha1Hex(password);
    const prefix = hash.slice(0, 5);
    const suffix = hash.slice(5);
    const res = await fetch(`https://api.pwnedpasswords.com/range/${prefix}`);
    if (!res.ok) return false;
    const text = await res.text();
    return text.split('\n').some((line) => line.split(':')[0].trim() === suffix);
  } catch {
    return false;
  }
}

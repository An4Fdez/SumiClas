const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;

export function isValidEmail(email: string): boolean {
  return EMAIL_REGEX.test(email.trim());
}

export const MIN_PASSWORD_LENGTH = 8;
export const RECOMMENDED_PASSWORD_LENGTH = 12;
export const MAX_PASSWORD_LENGTH = 128;
export const MAX_FULL_NAME_LENGTH = 200;
export const MAX_STUDENT_NAME_PART_LENGTH = 100;

async function sha1Hex(value: string): Promise<string> {
  const buffer = await crypto.subtle.digest('SHA-1', new TextEncoder().encode(value));
  return Array.from(new Uint8Array(buffer))
    .map((b) => b.toString(16).padStart(2, '0'))
    .join('')
    .toUpperCase();
}

/**
 * Checks the password against the public "Have I Been Pwned" API (Pwned Passwords)
 * using k-anonymity: only the first 5 characters of the SHA-1 hash are sent, never
 * the real password or the full hash. If the API doesn't respond, registration is
 * not blocked (fail-open) — this is an extra check, not the only barrier.
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

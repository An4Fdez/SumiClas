import { getSession } from './tokenStore';

const GUARDIAN_KEY = 'sumiclas-guardian';
const LEGACY_KEYS = ['sumiclas-user-id', 'sumiclas-auth'];

export type GuardianRole = 'father' | 'mother' | 'guardian';

export type GuardianInfo = {
  name: string;
  role: GuardianRole;
};

try {
  LEGACY_KEYS.forEach((key) => localStorage.removeItem(key));
} catch {
  // localStorage unavailable — ignored
}

export function getUserId(): string | null {
  return getSession()?.userId ?? null;
}

/** A session exists while there is an access token in memory. */
export function isAuthenticated(): boolean {
  return getSession() !== null;
}

export function getGuardianInfo(): GuardianInfo | null {
  try {
    const raw = localStorage.getItem(GUARDIAN_KEY);
    return raw ? (JSON.parse(raw) as GuardianInfo) : null;
  } catch {
    return null;
  }
}

export function setGuardianInfo(info: GuardianInfo) {
  try {
    localStorage.setItem(GUARDIAN_KEY, JSON.stringify(info));
  } catch {
    // localStorage unavailable — ignored
  }
}

export function clearGuardianInfo() {
  try {
    localStorage.removeItem(GUARDIAN_KEY);
  } catch {
    // localStorage unavailable — ignored
  }
}

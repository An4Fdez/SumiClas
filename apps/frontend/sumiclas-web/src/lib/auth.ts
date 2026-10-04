const GUARDIAN_KEY = 'sumiclas-guardian';
const USER_ID_KEY = 'sumiclas-user-id';
const LEGACY_AUTH_KEY = 'sumiclas-auth';

export type GuardianRole = 'father' | 'mother' | 'guardian';

export type GuardianInfo = {
  name: string;
  role: GuardianRole;
};

/** Provisional identity: the backend trusts this id (sent as X-User-Id) until JWT authentication exists. */
export function getUserId(): string | null {
  try {
    return localStorage.getItem(USER_ID_KEY);
  } catch {
    return null;
  }
}

export function setUserId(id: string) {
  try {
    localStorage.setItem(USER_ID_KEY, id);
  } catch {
    // localStorage unavailable — ignored
  }
}

/** A session exists while a user id is stored. */
export function isAuthenticated(): boolean {
  return getUserId() !== null;
}

export function clearSession() {
  try {
    localStorage.removeItem(USER_ID_KEY);
    localStorage.removeItem(GUARDIAN_KEY);
    localStorage.removeItem(LEGACY_AUTH_KEY);
  } catch {
    // localStorage unavailable — ignored
  }
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

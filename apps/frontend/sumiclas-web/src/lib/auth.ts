const AUTH_KEY = 'sumiclas-auth';
const GUARDIAN_KEY = 'sumiclas-guardian';
const USER_ID_KEY = 'sumiclas-user-id';

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

/** Backend-less prototype: the session is simulated with a flag in localStorage. */
export function isAuthenticated(): boolean {
  try {
    return localStorage.getItem(AUTH_KEY) === 'true';
  } catch {
    return false;
  }
}

export function setAuthenticated(value: boolean) {
  try {
    if (value) {
      localStorage.setItem(AUTH_KEY, 'true');
    } else {
      localStorage.removeItem(AUTH_KEY);
      localStorage.removeItem(USER_ID_KEY);
      localStorage.removeItem(GUARDIAN_KEY);
    }
  } catch {
    // localStorage unavailable (private mode, etc.) — ignored
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

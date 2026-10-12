const GUARDIAN_KEY = 'sumiclas-guardian';
const LEGACY_KEYS = ['sumiclas-user-id', 'sumiclas-auth'];

export type GuardianRole = 'father' | 'mother' | 'guardian';

export type GuardianInfo = {
  name: string;
  role: GuardianRole;
};

// Older builds kept the user id and an auth flag in localStorage; they must not outlive the move to in-memory tokens.
export function removeLegacySessionKeys() {
  try {
    LEGACY_KEYS.forEach((key) => localStorage.removeItem(key));
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

export function clearGuardianInfo() {
  try {
    localStorage.removeItem(GUARDIAN_KEY);
  } catch {
    // localStorage unavailable — ignored
  }
}

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

export const PASSWORD_CHANGED_NOTICE = 'Contraseña cambiada. Vuelve a iniciar sesión.';

const LOGIN_NOTICE_KEY = 'sumiclas-login-notice';

// sessionStorage, not router state: the redirect to /login that follows a local sign-out drops the state.
export function setLoginNotice(message: string) {
  try {
    sessionStorage.setItem(LOGIN_NOTICE_KEY, message);
  } catch {
    // sessionStorage unavailable — the notice is just not shown
  }
}

export function peekLoginNotice(): string | null {
  try {
    return sessionStorage.getItem(LOGIN_NOTICE_KEY);
  } catch {
    return null;
  }
}

export function clearLoginNotice() {
  try {
    sessionStorage.removeItem(LOGIN_NOTICE_KEY);
  } catch {
    // sessionStorage unavailable — ignored
  }
}

const DISPLAY_NAME_WORDS = 2;

/** The name and first surname of a full name, as the navbar shows the guardian. */
export function toGuardianDisplayName(fullName: string): string {
  return fullName.trim().split(/\s+/).slice(0, DISPLAY_NAME_WORDS).join(' ');
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

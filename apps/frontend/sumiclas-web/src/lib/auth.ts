const AUTH_KEY = 'sumiclas-auth';
const TUTOR_KEY = 'sumiclas-tutor';

export type TutorRole = 'padre' | 'madre' | 'tutor';

export type TutorInfo = {
  nombre: string;
  rol: TutorRole;
};

/** Prototipo sin backend: la sesión se simula con un flag en localStorage. */
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
      localStorage.removeItem(TUTOR_KEY);
    }
  } catch {
    // localStorage no disponible (modo privado, etc.) — se ignora
  }
}

export function getTutorInfo(): TutorInfo | null {
  try {
    const raw = localStorage.getItem(TUTOR_KEY);
    return raw ? (JSON.parse(raw) as TutorInfo) : null;
  } catch {
    return null;
  }
}

export function setTutorInfo(info: TutorInfo) {
  try {
    localStorage.setItem(TUTOR_KEY, JSON.stringify(info));
  } catch {
    // localStorage no disponible — se ignora
  }
}

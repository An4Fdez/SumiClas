import type { Session } from '@/modules/sessions/domain/Session';

let session: Session | null = null;
const listeners = new Set<() => void>();

function notify() {
  listeners.forEach((listener) => listener());
}

export function getSession(): Session | null {
  return session;
}

export function getAccessToken(): string | null {
  return session?.accessToken ?? null;
}

export function setSession(next: Session) {
  session = next;
  notify();
}

export function clearSession() {
  if (session === null) return;
  session = null;
  notify();
}

export function subscribe(listener: () => void): () => void {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

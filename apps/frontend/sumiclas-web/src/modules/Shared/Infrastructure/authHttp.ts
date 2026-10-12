import { getApiErrorStatus } from '@/lib/apiError';
import { clearSession, getAccessToken, setSession } from '@/lib/tokenStore';
import type { Session } from '@/modules/sessions/domain/Session';
import { reviveApiDates } from './apiDate';
import { GeneratedApiClient } from './GeneratedApiClient';

export const API_BASE_URL = '/api';

const REFRESH_LOCK_NAME = 'sumiclas-refresh';
const REFRESH_RETRY_DELAY_MS = 300;

class RefreshClient extends GeneratedApiClient {
  protected override jsonParseReviver = reviveApiDates;
}

const rawHttp = { fetch: (url: RequestInfo, init?: RequestInit) => globalThis.fetch(url, init) };

let sessionLostHandler: (() => void) | null = null;
let inFlightRefresh: Promise<Session> | null = null;

export function setSessionLostHandler(handler: (() => void) | null) {
  sessionLostHandler = handler;
}

function requestPath(url: RequestInfo | URL): string {
  const text = typeof url === 'string' ? url : url instanceof URL ? url.href : url.url;
  const path = new URL(text, 'http://localhost').pathname;
  return path.startsWith(`${API_BASE_URL}/`) ? path.slice(API_BASE_URL.length) : path;
}

function isAuthEndpoint(url: RequestInfo | URL): boolean {
  return requestPath(url).startsWith('/auth/');
}

function send(url: RequestInfo, init: RequestInit | undefined, token: string | null): Promise<Response> {
  if (!token) return globalThis.fetch(url, init);
  const headers = new Headers(init?.headers);
  headers.set('Authorization', `Bearer ${token}`);
  return globalThis.fetch(url, { ...init, headers });
}

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function rotateOnce(): Promise<Session> {
  const dto = await new RefreshClient(API_BASE_URL, rawHttp).sessionRotate();
  const session = { accessToken: dto.accessToken, expiresAt: dto.expiresAt, userId: dto.userId, fullName: dto.fullName };
  setSession(session);
  return session;
}

function isRetryable(error: unknown): boolean {
  const status = getApiErrorStatus(error);
  return status === 401 || status === 409;
}

async function rotateWithRetry(notifyOnLoss: boolean): Promise<Session> {
  try {
    return await rotateOnce();
  } catch (firstError) {
    if (!isRetryable(firstError)) throw firstError;
    // 401: another tab rotated the cookie first. 409: a simultaneous refresh won the race.
    await wait(REFRESH_RETRY_DELAY_MS);
    try {
      return await rotateOnce();
    } catch (secondError) {
      if (isRetryable(secondError)) {
        clearSession();
        if (notifyOnLoss) sessionLostHandler?.();
      }
      throw secondError;
    }
  }
}

/**
 * Single-flight: concurrent callers share one promise, and tabs are serialised with Web Locks when available.
 * Rejects with the last error; the session-lost handler runs only when the refresh was refused (401/409 twice),
 * never for network errors or 5xx, which say nothing about the session.
 */
export function refreshSession(options: { notifyOnLoss?: boolean } = {}): Promise<Session> {
  if (inFlightRefresh) return inFlightRefresh;
  const notifyOnLoss = options.notifyOnLoss ?? true;
  const locks = globalThis.navigator?.locks;
  const run = () => rotateWithRetry(notifyOnLoss);
  const attempt: Promise<Session> = locks ? Promise.resolve(locks.request(REFRESH_LOCK_NAME, run)) : run();
  const shared: Promise<Session> = attempt.finally(() => {
    if (inFlightRefresh === shared) inFlightRefresh = null;
  });
  inFlightRefresh = shared;
  return shared;
}

export const authHttp = {
  async fetch(url: RequestInfo, init?: RequestInit): Promise<Response> {
    const sentToken = getAccessToken();
    const response = await send(url, init, sentToken);
    if (response.status !== 401 || isAuthEndpoint(url)) return response;

    const currentToken = getAccessToken();
    if (currentToken && currentToken !== sentToken) return send(url, init, currentToken);

    let session: Session;
    try {
      session = await refreshSession();
    } catch {
      return response;
    }
    return send(url, init, session.accessToken);
  },
};

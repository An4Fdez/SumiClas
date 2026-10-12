import { clearSession, setSession } from '@/lib/tokenStore';
import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import { refreshSession } from '@/modules/Shared/Infrastructure/authHttp';
import type { ISessionRepository } from '../domain/ISessionRepository';
import type { NewSession } from '../domain/NewSession';
import type { Session } from '../domain/Session';

export class NSwagSessionRepository implements ISessionRepository {
  constructor(private readonly client: ApiClient) {}

  async login(newSession: NewSession): Promise<Session> {
    const dto = await this.client.sessionCreate({ email: newSession.email, password: newSession.password });
    const session = { accessToken: dto.accessToken, expiresAt: dto.expiresAt, userId: dto.userId, fullName: dto.fullName };
    setSession(session);
    return session;
  }

  /** Shares the single-flight refresh (and the cross-tab lock) with the 401 handling; a refusal never fires the session-lost handler. */
  refresh(): Promise<Session> {
    return refreshSession({ notifyOnLoss: false });
  }

  async logout(): Promise<void> {
    try {
      await this.client.sessionRevoke();
    } finally {
      clearSession();
    }
  }
}

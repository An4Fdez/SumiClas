import type { NewSession } from './NewSession';
import type { Session } from './Session';

export interface ISessionRepository {
  login(newSession: NewSession): Promise<Session>;
  refresh(): Promise<Session>;
  logout(): Promise<void>;
}

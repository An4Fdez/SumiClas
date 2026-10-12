import { createContext } from 'react';
import type { NewSession } from '@/modules/sessions/domain/NewSession';
import type { Session } from '@/modules/sessions/domain/Session';
import type { NewUser } from '@/modules/users/domain/NewUser';

export type AuthStatus = 'loading' | 'authenticated' | 'anonymous';

export type AuthUser = {
  userId: string;
  fullName: string;
};

/** The account was created but the automatic sign-in that follows (RF-20) failed. */
export class AccountCreatedLoginFailedError extends Error {
  constructor(readonly loginError: unknown) {
    super('The account was created but signing in failed.');
  }
}

export type AuthContextValue = {
  status: AuthStatus;
  user: AuthUser | null;
  login: (newSession: NewSession) => Promise<Session>;
  /** Throws AccountCreatedLoginFailedError when only the sign-in after the registration failed. */
  register: (newUser: NewUser) => Promise<void>;
  /** Never throws: the local session is cleared even when the server call fails. */
  logout: () => Promise<void>;
};

export const AuthContext = createContext<AuthContextValue | null>(null);

import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSessionRepository } from '@/hooks/useSessionRepository';
import { useUserRepository } from '@/hooks/useUserRepository';
import { clearGuardianInfo } from '@/lib/auth';
import { clearSession, getSession, subscribe } from '@/lib/tokenStore';
import { setSessionLostHandler } from '@/modules/Shared/Infrastructure/authHttp';
import type { NewSession } from '@/modules/sessions/domain/NewSession';
import type { NewUser } from '@/modules/users/domain/NewUser';
import { AccountCreatedLoginFailedError, AuthContext, type AuthContextValue, type AuthStatus, type AuthUser } from './AuthContext';

type AuthState = { status: AuthStatus; user: AuthUser | null };

const LOADING: AuthState = { status: 'loading', user: null };
const ANONYMOUS: AuthState = { status: 'anonymous', user: null };

function fromStore(previous: AuthState): AuthState {
  const session = getSession();
  if (!session) return previous.status === 'anonymous' ? previous : ANONYMOUS;
  if (previous.user?.userId === session.userId && previous.user.fullName === session.fullName) {
    return previous.status === 'authenticated' ? previous : { status: 'authenticated', user: previous.user };
  }
  return { status: 'authenticated', user: { userId: session.userId, fullName: session.fullName } };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const sessionRepository = useSessionRepository();
  const userRepository = useUserRepository();
  const navigate = useNavigate();
  const [state, setState] = useState<AuthState>(LOADING);

  // useNavigate changes identity with the location: keep it out of the effect that restores the session.
  const navigateRef = useRef(navigate);
  useEffect(() => {
    navigateRef.current = navigate;
  }, [navigate]);

  useEffect(() => {
    let ignore = false;
    const sync = () => {
      if (!ignore) setState(fromStore);
    };

    const unsubscribe = subscribe(sync);
    setSessionLostHandler(() => {
      sync();
      navigateRef.current('/login', { replace: true });
    });

    sessionRepository.refresh().then(sync, sync);

    return () => {
      ignore = true;
      unsubscribe();
      setSessionLostHandler(null);
    };
  }, [sessionRepository]);

  const login = useCallback(
    async (newSession: NewSession) => {
      const session = await sessionRepository.login(newSession);
      setState(fromStore);
      return session;
    },
    [sessionRepository],
  );

  const register = useCallback(
    async (newUser: NewUser) => {
      await userRepository.register(newUser);
      try {
        await sessionRepository.login({ email: newUser.email, password: newUser.password });
      } catch (error) {
        throw new AccountCreatedLoginFailedError(error);
      }
      setState(fromStore);
    },
    [sessionRepository, userRepository],
  );

  const logout = useCallback(async () => {
    try {
      await sessionRepository.logout();
    } catch {
      // The repository already cleared the local session; the server-side revocation is best effort.
    }
    clearGuardianInfo();
    setState(fromStore);
  }, [sessionRepository]);

  const signOutLocally = useCallback(() => {
    clearGuardianInfo();
    clearSession();
    setState(fromStore);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({ status: state.status, user: state.user, login, register, logout, signOutLocally }),
    [state, login, register, logout, signOutLocally],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

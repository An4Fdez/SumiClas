import { createContext } from 'react';
import type { IAuthenticationRepository } from '@/modules/authentication/domain/IAuthenticationRepository';

export const AuthenticationRepositoryContext = createContext<IAuthenticationRepository | null>(null);

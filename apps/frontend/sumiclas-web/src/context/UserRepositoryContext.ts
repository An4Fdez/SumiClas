import { createContext } from 'react';
import type { IUserRepository } from '@/modules/users/domain/IUserRepository';

export const UserRepositoryContext = createContext<IUserRepository | null>(null);

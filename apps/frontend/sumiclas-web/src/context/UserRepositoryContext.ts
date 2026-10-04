import { createContext } from 'react';
import type { UserRepository } from '@/modules/users/domain/UserRepository';

export const UserRepositoryContext = createContext<UserRepository | null>(null);

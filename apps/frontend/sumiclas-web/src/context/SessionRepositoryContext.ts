import { createContext } from 'react';
import type { ISessionRepository } from '@/modules/sessions/domain/ISessionRepository';

export const SessionRepositoryContext = createContext<ISessionRepository | null>(null);

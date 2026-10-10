import { createContext } from 'react';
import type { ILocalityRepository } from '@/modules/localities/domain/ILocalityRepository';

export const LocalityRepositoryContext = createContext<ILocalityRepository | null>(null);

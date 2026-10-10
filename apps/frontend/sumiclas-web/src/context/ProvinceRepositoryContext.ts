import { createContext } from 'react';
import type { IProvinceRepository } from '@/modules/provinces/domain/IProvinceRepository';

export const ProvinceRepositoryContext = createContext<IProvinceRepository | null>(null);

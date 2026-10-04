import { createContext } from 'react';
import type { ILostItemRepository } from '@/modules/lostItems/domain/ILostItemRepository';

export const LostItemRepositoryContext = createContext<ILostItemRepository | null>(null);

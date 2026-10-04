import { createContext } from 'react';
import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';

export const ApiClientContext = createContext<ApiClient | null>(null);

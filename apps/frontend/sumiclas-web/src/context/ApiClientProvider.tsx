import { useState, type ReactNode } from 'react';
import { createApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import { ApiClientContext } from './ApiClientContext';

export function ApiClientProvider({ children }: { children: ReactNode }) {
  const [client] = useState(createApiClient);

  return <ApiClientContext.Provider value={client}>{children}</ApiClientContext.Provider>;
}

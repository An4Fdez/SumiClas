import { useContext } from 'react';
import { ApiClientContext } from '@/context/ApiClientContext';

export function useApiClient() {
  const client = useContext(ApiClientContext);
  if (!client) throw new Error('useApiClient must be used within ApiClientProvider');
  return client;
}

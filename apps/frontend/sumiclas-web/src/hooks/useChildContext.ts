import { useContext } from 'react';
import { ChildContext } from '@/context/ChildContext';

export function useChildContext() {
  const ctx = useContext(ChildContext);
  if (!ctx) throw new Error('useChildContext must be used within ChildProvider');
  return ctx;
}

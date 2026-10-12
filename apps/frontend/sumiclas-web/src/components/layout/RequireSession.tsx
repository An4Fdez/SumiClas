import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { SessionLoading } from './SessionLoading';

export function RequireSession() {
  const { status } = useAuth();
  if (status === 'loading') return <SessionLoading />;
  if (status === 'anonymous') return <Navigate to="/login" replace />;
  return <Outlet />;
}

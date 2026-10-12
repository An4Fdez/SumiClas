import { Link, Outlet, useNavigate } from 'react-router-dom';
import { Moon, Sun } from 'lucide-react';
import { Logo } from './Logo';
import { ChildSelector } from '@/components/app/ChildSelector';
import { AppTabBar } from '@/components/app/AppTabBar';
import { ChildProvider } from '@/context/ChildProvider';
import { useChildContext } from '@/hooks/useChildContext';
import { useTheme } from '@/hooks/useTheme';
import { useAuth } from '@/hooks/useAuth';

function AppContent() {
  const navigate = useNavigate();
  const { logout } = useAuth();
  const { loading, error, reload } = useChildContext();

  if (loading) {
    return (
      <p role="status" className="py-16 text-center text-sm text-slate-500 dark:text-slate-400">
        Cargando tu información…
      </p>
    );
  }

  if (error) {
    const handleLogout = async () => {
      await logout();
      navigate('/login', { replace: true });
    };

    return (
      <div
        role="alert"
        className="mx-auto max-w-md rounded-2xl border border-rose-200 bg-rose-50 p-6 text-center dark:border-rose-700/40 dark:bg-rose-900/20"
      >
        <p className="text-sm text-rose-700 dark:text-rose-300">{error}</p>
        <div className="mt-4 flex justify-center gap-2">
          <button
            type="button"
            onClick={reload}
            className="rounded-xl bg-brand-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-brand-700"
          >
            Reintentar
          </button>
          <button
            type="button"
            onClick={handleLogout}
            className="rounded-xl border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-white dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800"
          >
            Cerrar sesión
          </button>
        </div>
      </div>
    );
  }

  return <Outlet />;
}

export function AppLayout() {
  const { theme, toggle } = useTheme();

  return (
    <ChildProvider>
      <div className="min-h-screen bg-slate-50 dark:bg-slate-950">
        <header className="sticky top-0 z-40 border-b border-slate-200/70 bg-white/80 backdrop-blur-xl dark:border-slate-800/70 dark:bg-slate-950/70">
          <div className="mx-auto flex max-w-7xl items-center gap-3 px-4 py-3 sm:px-6 lg:px-8">
            <Link to="/" className="shrink-0">
              <Logo />
            </Link>
            <div className="flex min-w-0 flex-1 justify-center">
              <ChildSelector />
            </div>
            <button
              onClick={toggle}
              aria-label="Cambiar tema"
              className="grid h-10 w-10 shrink-0 place-items-center rounded-lg border border-slate-200 text-slate-600 transition-colors hover:bg-slate-100 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800"
            >
              {theme === 'dark' ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
            </button>
          </div>
        </header>

        <main className="mx-auto max-w-7xl px-4 pb-24 pt-6 sm:px-6 lg:px-8">
          <AppContent />
        </main>

        <AppTabBar />
      </div>
    </ChildProvider>
  );
}

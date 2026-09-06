import { Link, Outlet } from 'react-router-dom';
import { Moon, Sun } from 'lucide-react';
import { Logo } from './Logo';
import { ChildSelector } from '@/components/app/ChildSelector';
import { AppTabBar } from '@/components/app/AppTabBar';
import { ChildProvider } from '@/context/ChildProvider';
import { useTheme } from '@/hooks/useTheme';

export function AppLayout() {
  const { theme, toggle } = useTheme();

  return (
    <ChildProvider>
      <div className="min-h-screen bg-slate-50 dark:bg-slate-950">
        <header className="sticky top-0 z-40 border-b border-slate-200 bg-white/90 backdrop-blur-xl dark:border-slate-800 dark:bg-slate-950/90">
          <div className="mx-auto flex max-w-5xl items-center gap-3 px-4 py-3 sm:px-6 lg:px-8">
            <Link to="/inicio" className="hidden shrink-0 sm:block">
              <Logo />
            </Link>
            <div className="min-w-0 flex-1">
              <ChildSelector />
            </div>
            <button
              onClick={toggle}
              aria-label="Cambiar tema"
              className="grid h-9 w-9 shrink-0 place-items-center rounded-lg border border-slate-200 text-slate-600 transition-colors hover:bg-slate-100 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800"
            >
              {theme === 'dark' ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
            </button>
          </div>
        </header>

        <main className="mx-auto max-w-5xl px-4 pb-24 pt-6 sm:px-6 lg:px-8">
          <Outlet />
        </main>

        <AppTabBar />
      </div>
    </ChildProvider>
  );
}

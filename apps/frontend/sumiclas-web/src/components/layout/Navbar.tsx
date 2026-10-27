import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Menu, X, Moon, Sun, LogIn, UserRound } from 'lucide-react';
import { Logo } from './Logo';
import { useTheme } from '@/hooks/useTheme';
import { useAuth } from '@/hooks/useAuth';
import { getGuardianInfo } from '@/lib/auth';

const links = [
  { label: 'Cómo funciona', href: '/#how-it-works' },
  { label: 'Funcionalidades', href: '/#features' },
  { label: 'Opiniones', href: '/#testimonials' },
  { label: 'Preguntas', href: '/#faq' },
];

export function Navbar() {
  const { theme, toggle } = useTheme();
  const { status } = useAuth();
  const [open, setOpen] = useState(false);
  // Layout stays mounted across the public routes, so subscribing to the location makes every navigation
  // re-read the guardian info from localStorage.
  useLocation();
  const isLoggedIn = status === 'authenticated';
  const guardian = isLoggedIn ? getGuardianInfo() : null;
  const guardianName = guardian?.name ?? 'Mi cuenta';

  return (
    <header className="fixed inset-x-0 top-0 z-50 border-b border-slate-200/70 bg-white/80 backdrop-blur-xl dark:border-slate-800/70 dark:bg-slate-950/70">
      <nav className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
        <Link to="/" className="shrink-0">
          <Logo />
        </Link>

        <ul className="hidden items-center gap-1 md:flex">
          {links.map((l) => (
            <li key={l.href}>
              <Link
                to={l.href}
                className="rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition-colors hover:bg-slate-100 hover:text-slate-900 dark:text-slate-300 dark:hover:bg-slate-800 dark:hover:text-white"
              >
                {l.label}
              </Link>
            </li>
          ))}
        </ul>

        <div className="flex items-center gap-2">
          <button
            onClick={toggle}
            aria-label="Cambiar tema"
            className="grid h-10 w-10 place-items-center rounded-lg border border-slate-200 text-slate-600 transition-colors hover:bg-slate-100 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800"
          >
            {theme === 'dark' ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
          </button>

          <Link
            to={isLoggedIn ? '/home' : '/login'}
            className="hidden items-center gap-1.5 rounded-lg px-3 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-100 hover:text-slate-900 dark:text-slate-300 dark:hover:bg-slate-800 dark:hover:text-white sm:flex"
          >
            {isLoggedIn ? <UserRound className="h-4 w-4" /> : <LogIn className="h-4 w-4" />}
            {isLoggedIn ? guardianName : 'Iniciar sesión'}
          </Link>

          <button
            onClick={() => setOpen((o) => !o)}
            aria-label="Menú"
            className="grid h-10 w-10 place-items-center rounded-lg border border-slate-200 text-slate-600 dark:border-slate-700 dark:text-slate-300 md:hidden"
          >
            {open ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </button>
        </div>
      </nav>

      {open && (
        <div className="border-t border-slate-200 bg-white/95 backdrop-blur-xl dark:border-slate-800 dark:bg-slate-950/95 md:hidden">
          <ul className="space-y-1 px-4 py-4">
            {links.map((l) => (
              <li key={l.href}>
                <Link
                  to={l.href}
                  onClick={() => setOpen(false)}
                  className="block rounded-lg px-3 py-2.5 text-base font-medium text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800"
                >
                  {l.label}
                </Link>
              </li>
            ))}
            <li>
              <Link
                to={isLoggedIn ? '/home' : '/login'}
                onClick={() => setOpen(false)}
                className="block rounded-lg px-3 py-2.5 text-base font-medium text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800"
              >
                {isLoggedIn ? guardianName : 'Iniciar sesión'}
              </Link>
            </li>
          </ul>
        </div>
      )}
    </header>
  );
}

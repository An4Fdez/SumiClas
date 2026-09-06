import { NavLink } from 'react-router-dom';
import { Home, Bell, Calendar, BookOpen, Wallet, PackageSearch, MoreHorizontal, type LucideIcon } from 'lucide-react';

const tabs: { to: string; label: string; icon: LucideIcon }[] = [
  { to: '/inicio', label: 'Inicio', icon: Home },
  { to: '/avisos', label: 'Avisos', icon: Bell },
  { to: '/calendario', label: 'Calendario', icon: Calendar },
  { to: '/deberes', label: 'Deberes', icon: BookOpen },
  { to: '/gastos', label: 'Gastos', icon: Wallet },
  { to: '/objetos-perdidos', label: 'Perdidos', icon: PackageSearch },
  { to: '/mas', label: 'Más', icon: MoreHorizontal },
];

export function AppTabBar() {
  return (
    <nav className="fixed inset-x-0 bottom-0 z-40 border-t border-slate-200 bg-white/95 backdrop-blur-xl dark:border-slate-800 dark:bg-slate-950/95">
      <div className="mx-auto flex max-w-7xl items-stretch justify-between px-4 sm:px-6 lg:px-8">
        {tabs.map((tab) => (
          <NavLink
            key={tab.to}
            to={tab.to}
            className={({ isActive }) =>
              `flex flex-1 flex-col items-center gap-1 py-2.5 text-[11px] font-semibold transition-colors ${
                isActive
                  ? 'text-brand-600 dark:text-brand-400'
                  : 'text-slate-400 hover:text-slate-600 dark:text-slate-500 dark:hover:text-slate-300'
              }`
            }
          >
            {({ isActive }) => (
              <>
                <tab.icon className="h-5 w-5" strokeWidth={isActive ? 2.4 : 2} />
                {tab.label}
              </>
            )}
          </NavLink>
        ))}
      </div>
    </nav>
  );
}

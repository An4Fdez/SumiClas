import { Link } from 'react-router-dom';
import { UserRound, Bell, HelpCircle, LogOut, ChevronRight, type LucideIcon } from 'lucide-react';
import { setAuthenticated } from '@/lib/auth';

const items: { icon: LucideIcon; label: string; href: string }[] = [
  { icon: UserRound, label: 'Mi perfil', href: '#perfil' },
  { icon: Bell, label: 'Notificaciones', href: '#notificaciones' },
  { icon: HelpCircle, label: 'Ayuda y soporte', href: '#ayuda' },
];

export default function Mas() {
  return (
    <div>
      <h1 className="font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
        Más
      </h1>

      <div className="mt-6 overflow-hidden rounded-2xl border border-slate-200 bg-white dark:border-slate-700 dark:bg-slate-900">
        {items.map((item) => (
          <a
            key={item.label}
            href={item.href}
            className="flex items-center gap-3 border-b border-slate-100 px-4 py-3.5 transition-colors last:border-b-0 hover:bg-slate-50 dark:border-slate-800 dark:hover:bg-slate-800"
          >
            <item.icon className="h-4 w-4 text-slate-400" />
            <span className="flex-1 text-sm font-medium text-slate-700 dark:text-slate-200">{item.label}</span>
            <ChevronRight className="h-4 w-4 text-slate-300 dark:text-slate-600" />
          </a>
        ))}
        <Link
          to="/"
          onClick={() => setAuthenticated(false)}
          className="flex items-center gap-3 px-4 py-3.5 text-rose-600 transition-colors hover:bg-rose-50 dark:text-rose-400 dark:hover:bg-rose-900/15"
        >
          <LogOut className="h-4 w-4" />
          <span className="flex-1 text-sm font-semibold">Cerrar sesión</span>
        </Link>
      </div>
    </div>
  );
}

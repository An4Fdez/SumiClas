import type { LucideIcon } from 'lucide-react';
import { badgeColorClasses, type PaletteColor } from '@/lib/colorClasses';

export function PlaceholderSection({
  icon: Icon,
  color,
  title,
  description,
}: {
  icon: LucideIcon;
  color: PaletteColor;
  title: string;
  description: string;
}) {
  const c = badgeColorClasses[color];
  return (
    <div>
      <div className={`grid h-12 w-12 place-items-center rounded-xl ${c.bg} ${c.text}`}>
        <Icon className="h-6 w-6" />
      </div>
      <h1 className="mt-4 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
        {title}
      </h1>
      <p className="mt-2 max-w-xl text-slate-600 dark:text-slate-400">{description}</p>
      <div className="mt-6 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-6 text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
        Esta sección todavía está en construcción.
      </div>
    </div>
  );
}

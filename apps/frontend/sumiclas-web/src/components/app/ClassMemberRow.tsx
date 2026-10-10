import type { ReactNode } from 'react';

export function ClassMemberRow({
  name,
  isSelf,
  children,
}: {
  name: string;
  isSelf: boolean;
  children?: ReactNode;
}) {
  return (
    <li className="flex flex-col gap-3 rounded-2xl border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900 sm:flex-row sm:items-center sm:justify-between">
      <p className="text-sm font-semibold text-slate-900 dark:text-white">
        {name}
        {isSelf && <span className="ml-2 text-xs font-medium text-slate-500 dark:text-slate-400">(tú)</span>}
      </p>
      {children && <div className="flex flex-wrap gap-2">{children}</div>}
    </li>
  );
}

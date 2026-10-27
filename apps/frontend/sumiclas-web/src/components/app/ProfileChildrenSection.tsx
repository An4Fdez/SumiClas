import { useState } from 'react';
import { Pencil } from 'lucide-react';
import { useChildContext } from '@/hooks/useChildContext';
import type { Child } from '@/modules/students/domain/Child';
import { ChildAvatar } from './ChildAvatar';
import { ChildNameModal } from './ChildNameModal';

export function ProfileChildrenSection() {
  const { children, refresh } = useChildContext();
  const [editing, setEditing] = useState<Child | null>(null);

  if (children.length === 0) {
    return <p className="text-sm text-slate-500 dark:text-slate-400">Todavía no tienes ningún hijo/a registrado.</p>;
  }

  return (
    <>
      <ul className="space-y-2">
        {children.map((child) => (
          <li
            key={child.id}
            className="flex items-center justify-between gap-3 rounded-xl border border-slate-200 p-3 dark:border-slate-700"
          >
            <div className="flex min-w-0 items-center gap-3">
              <ChildAvatar name={child.studentName} />
              <div className="min-w-0">
                <p className="truncate text-sm font-semibold text-slate-900 dark:text-white">{child.studentName}</p>
                <p className="truncate text-xs text-slate-500 dark:text-slate-400">
                  {[child.schoolName, child.className].filter(Boolean).join(', ')}
                </p>
              </div>
            </div>
            <button
              type="button"
              onClick={() => setEditing(child)}
              aria-label={`Editar el nombre de ${child.firstName}`}
              className="inline-flex shrink-0 items-center gap-1.5 rounded-xl border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800"
            >
              <Pencil className="h-4 w-4" />
              Editar
            </button>
          </li>
        ))}
      </ul>
      {editing && <ChildNameModal child={editing} onSaved={refresh} onClose={() => setEditing(null)} />}
    </>
  );
}

import { useState, type FocusEvent } from 'react';
import { ChevronDown } from 'lucide-react';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses } from '@/lib/colorClasses';
import { roleMeta } from '@/lib/roles';

function Avatar({ name }: { name: string }) {
  return (
    <div className="grid h-8 w-8 shrink-0 place-items-center rounded-full bg-gradient-to-br from-brand-400 via-mint-400 to-rose-400 font-display text-xs font-bold text-white">
      {name.charAt(0)}
    </div>
  );
}

function RoleBadge({ role }: { role: 'classRepresentative' | 'participant' }) {
  const meta = roleMeta[role];
  const c = badgeColorClasses[meta.color];
  return (
    <span
      className={`hidden shrink-0 rounded-full px-2 py-0.5 text-[11px] font-bold sm:inline-block ${c.bg} ${c.text}`}
    >
      {meta.label}
    </span>
  );
}

export function ChildSelector() {
  const { children, currentChild, setCurrentChildId, currentRole } = useChildContext();
  const [open, setOpen] = useState(false);

  if (!currentChild) return null;

  const label = (
    <span className="min-w-0 truncate text-sm text-slate-600 dark:text-slate-400">
      Viendo:{' '}
      <span className="font-semibold text-slate-900 dark:text-white">{currentChild.studentName}</span>
      {' — '}
      {currentChild.schoolName}, {currentChild.className}
    </span>
  );

  if (children.length <= 1) {
    return (
      <div className="flex min-w-0 items-center gap-2.5">
        <Avatar name={currentChild.studentName} />
        {label}
        {currentRole && <RoleBadge role={currentRole} />}
      </div>
    );
  }

  const handleBlur = (e: FocusEvent<HTMLDivElement>) => {
    if (!e.currentTarget.contains(e.relatedTarget as Node)) setOpen(false);
  };

  return (
    <div className="relative min-w-0" onBlur={handleBlur}>
      <button
        type="button"
        onClick={() => setOpen((o) => !o)}
        className="flex min-w-0 items-center gap-2.5 rounded-xl px-2 transition-colors hover:bg-slate-100 dark:hover:bg-slate-800"
      >
        <Avatar name={currentChild.studentName} />
        {label}
        {currentRole && <RoleBadge role={currentRole} />}
        <ChevronDown
          className={`h-4 w-4 shrink-0 text-slate-400 transition-transform ${open ? 'rotate-180' : ''}`}
        />
      </button>

      {open && (
        <div className="absolute left-0 top-full z-30 mt-2 w-72 max-w-[90vw] rounded-2xl border border-slate-200 bg-white p-1.5 shadow-xl dark:border-slate-700 dark:bg-slate-900">
          {children.map((c) => (
            <button
              key={c.id}
              type="button"
              onClick={() => {
                setCurrentChildId(c.id);
                setOpen(false);
              }}
              className={`flex w-full items-center gap-2.5 rounded-xl px-3 py-2.5 text-left transition-colors hover:bg-slate-50 dark:hover:bg-slate-800 ${
                c.id === currentChild.id ? 'bg-brand-50/60 dark:bg-brand-900/15' : ''
              }`}
            >
              <Avatar name={c.studentName} />
              <span className="min-w-0">
                <span className="block truncate text-sm font-semibold text-slate-900 dark:text-white">
                  {c.studentName}
                </span>
                <span className="block truncate text-xs text-slate-500 dark:text-slate-400">
                  {c.schoolName}, {c.className}
                </span>
              </span>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

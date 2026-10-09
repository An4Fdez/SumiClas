import { Check, MessageCircle } from 'lucide-react';
import { badgeColorClasses } from '@/lib/colorClasses';
import { formatDueDate } from '@/lib/date';
import { roleLabel } from '@/lib/roles';
import { subjectColor } from '@/lib/subjectColor';
import type { Assignment } from '@/modules/assignments/domain/Assignment';

export function AssignmentCard({
  assignment,
  done,
  onOpen,
  onToggleDone,
}: {
  assignment: Assignment;
  done: boolean;
  onOpen: () => void;
  onToggleDone: () => void;
}) {
  const c = badgeColorClasses[subjectColor(assignment.subjectId)];
  const commentCount = assignment.comments.length;

  return (
    <div
      className={`relative w-full rounded-2xl border border-slate-200 bg-white shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md dark:border-slate-700 dark:bg-slate-900 ${
        done ? 'opacity-60' : ''
      }`}
    >
      <button
        type="button"
        onClick={(e) => {
          e.stopPropagation();
          onToggleDone();
        }}
        aria-label={done ? 'Marcar como pendiente' : 'Marcar como hecho'}
        title={done ? 'Marcar como pendiente' : 'Marcar como hecho'}
        className={`absolute right-3 top-3 z-10 grid h-6 w-6 place-items-center rounded-full border transition-colors ${
          done
            ? 'border-mint-500 bg-mint-500 text-white'
            : 'border-slate-300 hover:bg-slate-100 dark:border-slate-600 dark:hover:bg-slate-800'
        }`}
      >
        {done && <Check className="h-3.5 w-3.5" />}
      </button>

      <button type="button" onClick={onOpen} className="w-full p-4 pr-11 text-left">
        <div className="flex flex-wrap items-center gap-2">
          <span className={`rounded-full px-2.5 py-0.5 text-[11px] font-bold ${c.bg} ${c.text}`}>
            {assignment.subjectLabel}
          </span>
          {assignment.isProposal && (
            <span className="rounded-full bg-slate-200 px-2.5 py-0.5 text-[11px] font-bold text-slate-600 dark:bg-slate-700 dark:text-slate-300">
              Propuesta
            </span>
          )}
        </div>

        <h3
          className={`mt-2.5 font-display text-base font-bold text-slate-900 dark:text-white ${
            done ? 'line-through' : ''
          }`}
        >
          {assignment.title}
        </h3>

        <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-xs text-slate-500 dark:text-slate-400">
          <span>
            {[assignment.authorName, roleLabel(assignment.authorRole)].filter(Boolean).join(' · ')}
          </span>
          <span className="flex items-center gap-2">
            {commentCount > 0 && (
              <span className="inline-flex items-center gap-1">
                <MessageCircle className="h-3.5 w-3.5" />
                {commentCount}
              </span>
            )}
            {formatDueDate(assignment.dueDate)}
          </span>
        </div>
      </button>
    </div>
  );
}

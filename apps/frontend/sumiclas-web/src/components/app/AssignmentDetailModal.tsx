import { useRef, useState, type FormEvent, type MouseEvent } from 'react';
import { Check, Send, X } from 'lucide-react';
import { badgeColorClasses } from '@/lib/colorClasses';
import { formatFullDay, formatRelativeDate } from '@/lib/date';
import type { SubjectInfo, AssignmentComment, Assignment } from '@/data/homeworkMock';

export function AssignmentDetailModal({
  assignment,
  subject,
  comments,
  done,
  onToggleDone,
  onAddComment,
  onClose,
}: {
  assignment: Assignment;
  subject: SubjectInfo;
  comments: AssignmentComment[];
  done: boolean;
  onToggleDone: () => void;
  onAddComment: (text: string) => void;
  onClose: () => void;
}) {
  const c = badgeColorClasses[subject.color];
  const mouseDownOnBackdrop = useRef(false);
  const [text, setText] = useState('');

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    const clean = text.trim();
    if (!clean) return;
    onAddComment(clean);
    setText('');
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-end justify-center bg-slate-900/40 p-0 backdrop-blur-sm sm:items-center sm:p-4"
      onMouseDown={(e: MouseEvent) => {
        mouseDownOnBackdrop.current = e.target === e.currentTarget;
      }}
      onClick={(e: MouseEvent) => {
        if (mouseDownOnBackdrop.current && e.target === e.currentTarget) onClose();
      }}
    >
      <div
        className="flex max-h-[85vh] w-full max-w-lg flex-col overflow-hidden rounded-t-3xl border border-slate-200 bg-white shadow-xl dark:border-slate-700 dark:bg-slate-900 sm:rounded-3xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="overflow-y-auto p-6">
          <div className="flex items-start justify-between gap-3">
            <div className="flex flex-wrap items-center gap-2">
              <span className={`rounded-full px-2.5 py-0.5 text-[11px] font-bold ${c.bg} ${c.text}`}>
                {subject.label}
              </span>
              {assignment.isProposal && (
                <span className="rounded-full bg-slate-200 px-2.5 py-0.5 text-[11px] font-bold text-slate-600 dark:bg-slate-700 dark:text-slate-300">
                  Propuesta
                </span>
              )}
            </div>
            <button
              type="button"
              onClick={onClose}
              className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
              aria-label="Cerrar"
            >
              <X className="h-5 w-5" />
            </button>
          </div>

          <h2 className="mt-3 font-display text-xl font-extrabold text-slate-900 dark:text-white">{assignment.title}</h2>
          <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
            {assignment.authorName} · {assignment.authorRole} · Entrega: {formatFullDay(new Date(assignment.dueDate))}
          </p>

          {assignment.description && (
            <p className="mt-4 whitespace-pre-line text-sm leading-relaxed text-slate-700 dark:text-slate-300">
              {assignment.description}
            </p>
          )}

          <button
            type="button"
            onClick={onToggleDone}
            className={`mt-4 inline-flex items-center gap-1.5 rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
              done
                ? 'border-mint-500 bg-mint-50 text-mint-700 dark:border-mint-500 dark:bg-mint-900/20 dark:text-mint-300'
                : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
            }`}
          >
            <Check className="h-4 w-4" />
            {done ? 'Hecho' : 'Marcar como hecho'}
          </button>

          <div className="mt-6">
            <h3 className="text-sm font-bold text-slate-900 dark:text-white">
              {comments.length > 0 ? `${comments.length} comentarios` : 'Sin comentarios todavía'}
            </h3>
            {comments.length > 0 && (
              <div className="mt-3 space-y-3">
                {comments.map((com) => (
                  <div key={com.id} className="rounded-xl bg-slate-50 p-3 dark:bg-slate-800">
                    <p className="text-xs font-semibold text-slate-700 dark:text-slate-300">
                      {com.authorName} · {com.authorRole} · {formatRelativeDate(com.date)}
                    </p>
                    <p className="mt-1 text-sm text-slate-700 dark:text-slate-300">{com.text}</p>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        <form
          onSubmit={handleSubmit}
          className="flex items-center gap-2 border-t border-slate-200 p-3 dark:border-slate-700"
        >
          <input
            value={text}
            onChange={(e) => setText(e.target.value)}
            maxLength={200}
            placeholder="Escribe una duda corta..."
            className="flex-1 rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white"
          />
          <button
            type="submit"
            disabled={!text.trim()}
            className="grid h-10 w-10 shrink-0 place-items-center rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 text-white disabled:cursor-not-allowed disabled:opacity-50"
            aria-label="Enviar comentario"
          >
            <Send className="h-4 w-4" />
          </button>
        </form>
      </div>
    </div>
  );
}

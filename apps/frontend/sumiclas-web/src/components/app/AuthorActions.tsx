import { useEffect, useRef, useState } from 'react';
import { Info, Pencil, Trash2 } from 'lucide-react';
import { getApiErrorMessage } from '@/lib/apiError';
import { RepresentativeConfirm } from './RepresentativeConfirm';

/** Edit and delete buttons of what the current user published; delete asks for confirmation first. */
export function AuthorActions({
  deleteTitle,
  deleteMessage,
  deleteBlockedReason,
  onEdit,
  onDelete,
}: {
  deleteTitle: string;
  deleteMessage: string;
  /** When given, the thing cannot be deleted: the button is replaced by this explanation. */
  deleteBlockedReason?: string;
  /** Left out when the thing can be deleted but not edited (a birthday). */
  onEdit?: () => void;
  onDelete: () => Promise<void>;
}) {
  const [confirmingDelete, setConfirmingDelete] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const errorRef = useRef<HTMLParagraphElement>(null);

  useEffect(() => {
    if (error) errorRef.current?.scrollIntoView({ block: 'nearest' });
  }, [error]);

  const handleDelete = async () => {
    if (deleting) return;
    setDeleting(true);
    setError(null);
    try {
      await onDelete();
    } catch (err) {
      setError(getApiErrorMessage(err));
      setDeleting(false);
    }
  };

  return (
    <div className="mt-6 border-t border-slate-200 pt-4 dark:border-slate-700">
      {confirmingDelete && !deleteBlockedReason ? (
        <RepresentativeConfirm
          title={deleteTitle}
          message={deleteMessage}
          confirmLabel={deleting ? 'Eliminando…' : 'Sí, eliminar'}
          onConfirm={handleDelete}
          onCancel={() => setConfirmingDelete(false)}
        />
      ) : (
        <div className="flex flex-wrap gap-2">
          {onEdit && (
            <button
              type="button"
              onClick={onEdit}
              className="inline-flex items-center gap-1.5 rounded-xl border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-700 transition-colors hover:bg-slate-50 dark:border-slate-700 dark:text-slate-200 dark:hover:bg-slate-800"
            >
              <Pencil className="h-4 w-4" />
              Editar
            </button>
          )}
          {!deleteBlockedReason && (
            <button
              type="button"
              onClick={() => setConfirmingDelete(true)}
              className="inline-flex items-center gap-1.5 rounded-xl border border-rose-200 px-3 py-2 text-sm font-semibold text-rose-600 transition-colors hover:bg-rose-50 dark:border-rose-800 dark:text-rose-300 dark:hover:bg-rose-900/20"
            >
              <Trash2 className="h-4 w-4" />
              Eliminar
            </button>
          )}
        </div>
      )}
      {deleteBlockedReason && (
        <p className="mt-3 flex items-start gap-2 text-sm text-slate-600 dark:text-slate-300">
          <Info className="mt-0.5 h-4 w-4 shrink-0 text-brand-500 dark:text-brand-400" />
          {deleteBlockedReason}
        </p>
      )}
      {error && (
        <p ref={errorRef} role="alert" className="mt-3 text-sm font-semibold text-rose-600 dark:text-rose-400">
          {error}
        </p>
      )}
    </div>
  );
}

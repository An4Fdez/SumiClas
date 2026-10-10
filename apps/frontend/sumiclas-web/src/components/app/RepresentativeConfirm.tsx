export function RepresentativeConfirm({
  title,
  message,
  confirmLabel,
  onConfirm,
  onCancel,
}: {
  title: string;
  message: string;
  confirmLabel: string;
  onConfirm: () => void;
  onCancel: () => void;
}) {
  return (
    <div
      role="alertdialog"
      aria-label={title}
      className="mt-4 rounded-2xl border border-sun-200 bg-sun-50/60 p-4 dark:border-sun-700/40 dark:bg-sun-900/15"
    >
      <p className="text-sm font-semibold text-slate-900 dark:text-white">{title}</p>
      <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">{message}</p>
      <div className="mt-3 flex flex-wrap gap-2">
        <button
          type="button"
          onClick={onConfirm}
          className="rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-3 py-2 text-sm font-semibold text-white shadow-sm shadow-brand-500/25"
        >
          {confirmLabel}
        </button>
        <button
          type="button"
          onClick={onCancel}
          className="rounded-xl border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800"
        >
          Cancelar
        </button>
      </div>
    </div>
  );
}

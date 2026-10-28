export function PendingClaimsBadge({ count }: { count: number }) {
  return (
    <span className="rounded-full bg-rose-100 px-2.5 py-0.5 text-[11px] font-bold text-rose-700 dark:bg-rose-900/40 dark:text-rose-300">
      {count} {count === 1 ? 'reclamación pendiente' : 'reclamaciones pendientes'}
    </span>
  );
}

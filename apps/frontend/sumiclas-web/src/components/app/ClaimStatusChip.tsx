import type { ClaimStatus } from '@/modules/lostItems/domain/ClaimStatus';

const LABELS: Record<ClaimStatus, string> = {
  pending: 'Reclamación enviada',
  accepted: 'Aceptada',
  rejected: 'Rechazada',
};

const CLASSES: Record<ClaimStatus, string> = {
  pending: 'bg-sun-100 text-sun-700 dark:bg-sun-900/40 dark:text-sun-300',
  accepted: 'bg-mint-100 text-mint-700 dark:bg-mint-900/40 dark:text-mint-300',
  rejected: 'bg-slate-200 text-slate-600 dark:bg-slate-700 dark:text-slate-300',
};


export function ClaimStatusChip({ status }: { status: ClaimStatus }) {
  return <span className={`rounded-full px-2.5 py-0.5 text-[11px] font-bold ${CLASSES[status]}`}>{LABELS[status]}</span>;
}

export function PendingClaimsBadge({ count }: { count: number }) {
  return (
    <span className="rounded-full bg-rose-100 px-2.5 py-0.5 text-[11px] font-bold text-rose-700 dark:bg-rose-900/40 dark:text-rose-300">
      {count} {count === 1 ? 'reclamación pendiente' : 'reclamaciones pendientes'}
    </span>
  );
}

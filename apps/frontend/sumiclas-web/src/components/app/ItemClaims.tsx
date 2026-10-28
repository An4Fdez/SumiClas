import { useState } from 'react';
import { Check, HandHeart, X } from 'lucide-react';
import { ClaimStatusChip } from '@/components/app/ClaimStatusChip';
import { ClaimTextForm } from '@/components/app/ClaimTextForm';
import { RepresentativeConfirm } from '@/components/app/RepresentativeConfirm';
import { useLostItemClaims } from '@/hooks/useLostItemClaims';
import { getApiErrorMessage } from '@/lib/apiError';
import { formatRelativeDate } from '@/lib/date';
import { roleLabel } from '@/lib/roles';
import { CLAIM_MESSAGE_MAX_LENGTH, CLAIM_RESPONSE_MAX_LENGTH } from '@/modules/lostItems/domain/ClaimLimits';
import type { LostItem } from '@/modules/lostItems/domain/LostItem';
import type { LostItemClaim } from '@/modules/lostItems/domain/LostItemClaim';

const CLAIM_COPY = {
  lost: { action: 'Creo que lo he visto / lo tengo yo', title: 'Cuéntale al autor lo que sabes' },
  found: { action: 'Es de mi hijo/a', title: 'Indica por qué es tuyo' },
} as const;

const CLAIM_HINT = 'Describe un detalle que no aparezca en la publicación (una marca, el color, algo que lleve dentro).';
const ACCEPT_HINT = 'Si quieres, deja una frase para coordinar la entrega. Solo la verá quien reclama.';

type Resolving = { claimId: string; kind: 'accept' | 'reject' } | null;

export function ItemClaims({ item, onChanged }: { item: LostItem; onChanged: (patch: Partial<LostItem>) => void }) {
  const { state, reload, createClaim, acceptClaim, rejectClaim } = useLostItemClaims(item, onChanged);
  const [claiming, setClaiming] = useState(false);
  const [resolving, setResolving] = useState<Resolving>(null);
  const [resolveError, setResolveError] = useState<string | null>(null);

  if (state.status === 'loading') {
    return (
      <p role="status" className="mt-6 text-sm text-slate-500 dark:text-slate-400">
        Cargando las reclamaciones…
      </p>
    );
  }

  if (state.status === 'error') {
    return (
      <div role="alert" className="mt-6 text-sm text-slate-600 dark:text-slate-300">
        <p>{state.message}</p>
        <button
          type="button"
          onClick={reload}
          className="mt-2 font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
        >
          Reintentar
        </button>
      </div>
    );
  }

  const claims = state.claims;

  const handleCreate = async (message: string) => {
    await createClaim(message);
    setClaiming(false);
  };

  const handleAccept = async (claim: LostItemClaim, responseMessage: string) => {
    await acceptClaim(claim.id, responseMessage);
    setResolving(null);
  };

  const handleReject = async (claim: LostItemClaim) => {
    setResolveError(null);
    try {
      await rejectClaim(claim.id);
    } catch (error) {
      setResolveError(getApiErrorMessage(error));
      return;
    }
    setResolving(null);
  };

  if (item.isAuthor) {
    if (claims.length === 0) return null;
    return (
      <section aria-label="Reclamaciones" className="mt-6 border-t border-slate-200 pt-4 dark:border-slate-700">
        <h3 className="font-display text-base font-bold text-slate-900 dark:text-white">Reclamaciones</h3>
        <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">Solo las ves tú y quien reclama.</p>
        {resolveError && (
          <p role="alert" className="mt-2 text-sm font-semibold text-rose-600 dark:text-rose-400">
            {resolveError}
          </p>
        )}
        <ul className="mt-3 space-y-3">
          {claims.map((claim) => {
            const isResolving = resolving?.claimId === claim.id;
            return (
              <li key={claim.id} className="rounded-2xl border border-slate-200 p-4 dark:border-slate-700">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="text-sm font-semibold text-slate-900 dark:text-white">
                    {[claim.claimantName, roleLabel(claim.claimantRole)].filter(Boolean).join(' · ')}
                  </span>
                  <ClaimStatusChip status={claim.status} />
                </div>
                <p className="mt-2 whitespace-pre-line text-sm text-slate-700 dark:text-slate-300">{claim.message}</p>
                <p className="mt-2 text-xs text-slate-500 dark:text-slate-400">{formatRelativeDate(claim.createdAt)}</p>
                {claim.responseMessage && (
                  <p className="mt-2 text-xs text-slate-600 dark:text-slate-400">
                    Tu mensaje para quedar: {claim.responseMessage}
                  </p>
                )}

                {claim.status === 'pending' && !isResolving && (
                  <div className="mt-3 flex flex-wrap gap-2">
                    <button
                      type="button"
                      onClick={() => {
                        setResolveError(null);
                        setResolving({ claimId: claim.id, kind: 'accept' });
                      }}
                      className="inline-flex items-center gap-1.5 rounded-xl border border-mint-500 px-3 py-2 text-sm font-semibold text-mint-700 transition-colors hover:bg-mint-50 dark:text-mint-300 dark:hover:bg-mint-900/20"
                    >
                      <Check className="h-4 w-4" />
                      Aceptar
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        setResolveError(null);
                        setResolving({ claimId: claim.id, kind: 'reject' });
                      }}
                      className="inline-flex items-center gap-1.5 rounded-xl border border-rose-200 px-3 py-2 text-sm font-semibold text-rose-600 transition-colors hover:bg-rose-50 dark:border-rose-800 dark:text-rose-300 dark:hover:bg-rose-900/20"
                    >
                      <X className="h-4 w-4" />
                      Rechazar
                    </button>
                  </div>
                )}

                {isResolving && resolving.kind === 'accept' && (
                  <ClaimTextForm
                    title="Aceptar esta reclamación"
                    hint={ACCEPT_HINT}
                    label="Mensaje para quedar (opcional)"
                    placeholder="p. ej. mañana a la salida"
                    maxLength={CLAIM_RESPONSE_MAX_LENGTH}
                    required={false}
                    submitLabel="Aceptar y marcar como recuperado"
                    submittingLabel="Aceptando…"
                    onSubmit={(text) => handleAccept(claim, text)}
                    onCancel={() => setResolving(null)}
                  />
                )}

                {isResolving && resolving.kind === 'reject' && (
                  <RepresentativeConfirm
                    title="¿Rechazar esta reclamación?"
                    message="Quien reclama verá que se ha rechazado."
                    confirmLabel="Sí, rechazar"
                    onConfirm={() => handleReject(claim)}
                    onCancel={() => setResolving(null)}
                  />
                )}
              </li>
            );
          })}
        </ul>
      </section>
    );
  }

  const latest = claims[0];
  const canClaim = !item.recovered && latest?.status !== 'pending';
  const copy = CLAIM_COPY[item.status];

  if (!latest && !canClaim) return null;

  return (
    <section aria-label="Tu reclamación" className="mt-6 border-t border-slate-200 pt-4 dark:border-slate-700">
      {latest && (
        <div className="rounded-2xl border border-slate-200 p-4 dark:border-slate-700">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span className="text-sm font-semibold text-slate-900 dark:text-white">Tu reclamación</span>
            <ClaimStatusChip status={latest.status} />
          </div>
          <p className="mt-2 whitespace-pre-line text-sm text-slate-700 dark:text-slate-300">{latest.message}</p>
          <p className="mt-2 text-xs text-slate-500 dark:text-slate-400">{formatRelativeDate(latest.createdAt)}</p>
          {latest.status === 'pending' && (
            <p className="mt-2 text-xs text-slate-600 dark:text-slate-400">
              El autor la revisará. Solo la veis tú y él/ella.
            </p>
          )}
          {latest.status === 'accepted' && (
            <p className="mt-2 text-sm font-semibold text-mint-700 dark:text-mint-300">
              {latest.responseMessage
                ? `Mensaje del autor: ${latest.responseMessage}`
                : 'El autor ha aceptado tu reclamación.'}
            </p>
          )}
          {latest.status === 'rejected' && (
            <p className="mt-2 text-xs text-slate-600 dark:text-slate-400">
              El autor no ha podido confirmar esta reclamación.
            </p>
          )}
        </div>
      )}

      {canClaim && !claiming && (
        <button
          type="button"
          onClick={() => setClaiming(true)}
          className="mt-3 inline-flex items-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-3.5 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25"
        >
          <HandHeart className="h-4 w-4" />
          {copy.action}
        </button>
      )}

      {canClaim && claiming && (
        <ClaimTextForm
          title={copy.title}
          hint={CLAIM_HINT}
          label="Tu mensaje"
          maxLength={CLAIM_MESSAGE_MAX_LENGTH}
          required
          submitLabel="Enviar reclamación"
          submittingLabel="Enviando…"
          onSubmit={handleCreate}
          onCancel={() => setClaiming(false)}
        />
      )}
    </section>
  );
}

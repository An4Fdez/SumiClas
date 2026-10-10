import { useState } from 'react';
import { ShieldCheck, ArrowLeftRight, UserPlus, LogOut } from 'lucide-react';
import { ClassMemberRow } from '@/components/app/ClassMemberRow';
import { RepresentativeConfirm } from '@/components/app/RepresentativeConfirm';
import { useChildContext } from '@/hooks/useChildContext';
import { useClassRepresentatives } from '@/hooks/useClassRepresentatives';
import { badgeColorClasses } from '@/lib/colorClasses';
import { roleMeta, canManageClassRepresentatives, MAX_CLASS_REPRESENTATIVES } from '@/lib/roles';
import type { ClassMember } from '@/modules/users/domain/ClassMember';

type Confirming = { kind: 'transfer'; member: ClassMember } | { kind: 'resign' } | null;

const secondaryButton =
  'inline-flex items-center gap-1.5 rounded-xl border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800';
const primaryButton =
  'inline-flex items-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-3 py-2 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 disabled:cursor-not-allowed disabled:opacity-50';

function EmptyNotice({ children }: { children: string }) {
  return (
    <div className="rounded-2xl border border-dashed border-slate-300 bg-white p-8 text-center dark:border-slate-700 dark:bg-slate-900">
      <p className="text-sm text-slate-600 dark:text-slate-400">{children}</p>
    </div>
  );
}

export default function MyClassRole() {
  const { currentChild, currentRole, currentUserId } = useChildContext();
  const classId = currentChild?.classId;
  const { state, reload, busy, actionError, resignPending, assign, transfer, resign, dismissError } =
    useClassRepresentatives(classId);
  const [confirming, setConfirming] = useState<Confirming>(null);

  if (!currentChild) return <EmptyNotice>Todavía no tienes ningún hijo/a registrado.</EmptyNotice>;
  if (!classId || !currentRole) {
    return <EmptyNotice>Tu hijo/a todavía no está matriculado/a en ninguna clase, así que no hay delegados que mostrar.</EmptyNotice>;
  }

  const meta = roleMeta[currentRole];
  const c = badgeColorClasses[meta.color];
  const canManage = canManageClassRepresentatives(currentRole);
  const members = state.status === 'ready' ? state.members : [];
  const representatives = members.filter((m) => m.role === 'classRepresentative');
  const participants = members.filter((m) => m.role !== 'classRepresentative');
  const others = participants.filter((m) => m.userId !== currentUserId);
  const atMax = representatives.length >= MAX_CLASS_REPRESENTATIVES;
  const onlyRepresentative = representatives.length <= 1;
  const working = busy !== null;

  const confirmAction = () => {
    const pending = confirming;
    setConfirming(null);
    if (pending?.kind === 'transfer') void transfer(pending.member.userId);
    if (pending?.kind === 'resign') void resign();
  };

  return (
    <div>
      <div className={`grid h-12 w-12 place-items-center rounded-xl ${c.bg} ${c.text}`}>
        <ShieldCheck className="h-6 w-6" />
      </div>
      <h1 className="mt-4 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
        Mi rol en la clase
      </h1>
      <p className="mt-2 max-w-xl text-slate-600 dark:text-slate-400">
        {currentChild.schoolName}, {currentChild.className}
      </p>

      <div className="mt-6 rounded-2xl border border-slate-200 bg-white p-5 dark:border-slate-700 dark:bg-slate-900">
        <div className="flex items-center gap-3">
          <span className={`rounded-full px-3 py-1 text-sm font-bold ${c.bg} ${c.text}`}>{meta.label}</span>
          {state.status === 'ready' && (
            <span className="text-sm text-slate-500 dark:text-slate-400">
              Delegados de esta clase: {representatives.length}/{MAX_CLASS_REPRESENTATIVES}
            </span>
          )}
        </div>
        <p className="mt-3 text-sm text-slate-600 dark:text-slate-400">{meta.description}</p>
        {canManage && state.status === 'ready' && (
          <div className="mt-4">
            <button
              type="button"
              disabled={working || onlyRepresentative}
              onClick={() => setConfirming({ kind: 'resign' })}
              className={secondaryButton}
            >
              <LogOut className="h-4 w-4" />
              {busy?.kind === 'resign' ? 'Renunciando…' : 'Renunciar como delegado/a'}
            </button>
            {onlyRepresentative && (
              <p className="mt-2 text-xs text-slate-500 dark:text-slate-400">
                Eres el único delegado/a de la clase: antes tienes que nombrar a otra persona.
              </p>
            )}
          </div>
        )}
        {confirming?.kind === 'resign' && (
          <RepresentativeConfirm
            title="¿Renunciar como delegado/a?"
            message="Pasarás a ser participante de la clase y dejarás de poder publicar avisos y eventos oficiales."
            confirmLabel="Sí, renunciar"
            onConfirm={confirmAction}
            onCancel={() => setConfirming(null)}
          />
        )}
      </div>

      {actionError && (
        <div
          role="alert"
          className="mt-4 rounded-2xl border border-rose-200 bg-rose-50/60 p-4 dark:border-rose-700/40 dark:bg-rose-900/15"
        >
          <p className="text-sm text-slate-700 dark:text-slate-300">{actionError}</p>
          <div className="mt-3 flex flex-wrap gap-2">
            {resignPending && (
              <button type="button" disabled={working} onClick={() => void resign()} className={primaryButton}>
                Reintentar la renuncia
              </button>
            )}
            <button type="button" onClick={dismissError} className={secondaryButton}>
              Cerrar
            </button>
          </div>
        </div>
      )}

      {state.status === 'loading' && (
        <p className="mt-6 text-sm text-slate-500 dark:text-slate-400">Cargando las familias de la clase…</p>
      )}

      {state.status === 'error' && (
        <div
          role="alert"
          className="mt-6 rounded-2xl border border-rose-200 bg-rose-50/60 p-4 dark:border-rose-700/40 dark:bg-rose-900/15"
        >
          <p className="text-sm text-slate-700 dark:text-slate-300">{state.message}</p>
          <button
            type="button"
            onClick={reload}
            className="mt-3 text-sm font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
          >
            Reintentar
          </button>
        </div>
      )}

      {state.status === 'ready' && (
        <>
          <div className="mt-6">
            <h2 className="font-display text-lg font-bold text-slate-900 dark:text-white">
              Delegados actuales de la clase
            </h2>
            {representatives.length === 0 ? (
              <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
                Esta clase no tiene delegado todavía.
              </p>
            ) : (
              <ul className="mt-3 space-y-2">
                {representatives.map((m) => (
                  <ClassMemberRow key={m.userId} name={m.fullName} isSelf={m.userId === currentUserId} />
                ))}
              </ul>
            )}
          </div>

          <div className="mt-6">
            <h2 className="font-display text-lg font-bold text-slate-900 dark:text-white">
              {canManage ? 'Gestionar delegados' : 'Familias de la clase'}
            </h2>
            {canManage && atMax && (
              <p className="mt-2 text-xs text-slate-500 dark:text-slate-400">
                La clase ya tiene {MAX_CLASS_REPRESENTATIVES} delegados. Para nombrar o ceder el rol a otra persona,
                antes tiene que dejar de serlo alguien.
              </p>
            )}
            {participants.length === 0 ? (
              <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
                No hay más familias participantes en esta clase todavía.
              </p>
            ) : (
              <ul className="mt-3 space-y-3">
                {(canManage ? others : participants).map((m) => (
                  <ClassMemberRow key={m.userId} name={m.fullName} isSelf={m.userId === currentUserId}>
                    {canManage && (
                      <>
                        <button
                          type="button"
                          disabled={working || atMax}
                          onClick={() => setConfirming({ kind: 'transfer', member: m })}
                          className={secondaryButton}
                        >
                          <ArrowLeftRight className="h-4 w-4" />
                          {busy?.kind === 'transfer' && busy.userId === m.userId ? 'Cediendo…' : 'Ceder el rol'}
                        </button>
                        <button
                          type="button"
                          disabled={working || atMax}
                          onClick={() => void assign(m.userId)}
                          className={primaryButton}
                        >
                          <UserPlus className="h-4 w-4" />
                          {busy?.kind === 'assign' && busy.userId === m.userId ? 'Nombrando…' : 'Hacer delegado/a'}
                        </button>
                      </>
                    )}
                  </ClassMemberRow>
                ))}
              </ul>
            )}
            {confirming?.kind === 'transfer' && (
              <RepresentativeConfirm
                title={`¿Ceder tu rol a ${confirming.member.fullName}?`}
                message="Esa persona pasará a ser delegado/a y tú dejarás de serlo: perderás la posibilidad de publicar avisos y eventos oficiales."
                confirmLabel="Sí, ceder el rol"
                onConfirm={confirmAction}
                onCancel={() => setConfirming(null)}
              />
            )}
          </div>
        </>
      )}
    </div>
  );
}

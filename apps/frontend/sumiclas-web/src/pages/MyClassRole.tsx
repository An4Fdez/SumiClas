import { ShieldCheck, ArrowLeftRight, UserPlus, LogOut } from 'lucide-react';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses } from '@/lib/colorClasses';
import { roleMeta, canManageClassRepresentatives, MAX_CLASS_REPRESENTATIVES } from '@/lib/roles';

export default function MyClassRole() {
  const {
    currentChild,
    currentRole,
    otherGuardians,
    transferClassRepresentative,
    assignClassRepresentative,
    resignClassRepresentative,
  } = useChildContext();

  if (!currentChild || !currentRole) {
    return (
      <div className="rounded-2xl border border-dashed border-slate-300 bg-white p-8 text-center dark:border-slate-700 dark:bg-slate-900">
        <p className="text-sm text-slate-600 dark:text-slate-400">Todavía no tienes ningún hijo/a registrado.</p>
      </div>
    );
  }

  const meta = roleMeta[currentRole];
  const c = badgeColorClasses[meta.color];
  const representativeCount =
    otherGuardians.filter((g) => g.role === 'classRepresentative').length +
    (currentRole === 'classRepresentative' ? 1 : 0);
  const canManage = canManageClassRepresentatives(currentRole);
  const participants = otherGuardians.filter((g) => g.role === 'participant');
  const currentRepresentatives = otherGuardians.filter((g) => g.role === 'classRepresentative');

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
          <span className="text-sm text-slate-500 dark:text-slate-400">
            Delegados de esta clase: {representativeCount}/{MAX_CLASS_REPRESENTATIVES}
          </span>
        </div>
        <p className="mt-3 text-sm text-slate-600 dark:text-slate-400">{meta.description}</p>
        {currentRole === 'classRepresentative' && currentRepresentatives.length >= 1 && (
          <button
            type="button"
            onClick={resignClassRepresentative}
            className="mt-4 inline-flex items-center gap-1.5 rounded-xl border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800"
          >
            <LogOut className="h-4 w-4" />
            Dejar de ser delegado/a
          </button>
        )}
      </div>

      <div className="mt-6">
        <h2 className="font-display text-lg font-bold text-slate-900 dark:text-white">
          Delegados actuales de la clase
        </h2>
        {representativeCount === 0 ? (
          <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">Esta clase no tiene delegado todavía.</p>
        ) : (
          <ul className="mt-3 space-y-2">
            {currentRole === 'classRepresentative' && (
              <li className="rounded-2xl border border-slate-200 bg-white p-4 text-sm font-semibold text-slate-900 dark:border-slate-700 dark:bg-slate-900 dark:text-white">
                Tú
              </li>
            )}
            {currentRepresentatives.map((g) => (
              <li
                key={g.id}
                className="rounded-2xl border border-slate-200 bg-white p-4 text-sm font-semibold text-slate-900 dark:border-slate-700 dark:bg-slate-900 dark:text-white"
              >
                {g.name}
              </li>
            ))}
          </ul>
        )}
      </div>

      {canManage && (
        <div className="mt-6">
          <h2 className="font-display text-lg font-bold text-slate-900 dark:text-white">Gestionar delegados</h2>
          {participants.length === 0 ? (
            <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
              No hay más familias participantes en esta clase todavía.
            </p>
          ) : (
            <div className="mt-3 space-y-3">
              {participants.map((g) => (
                <div
                  key={g.id}
                  className="flex flex-col gap-3 rounded-2xl border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900 sm:flex-row sm:items-center sm:justify-between"
                >
                  <p className="text-sm font-semibold text-slate-900 dark:text-white">{g.name}</p>
                  <div className="flex flex-wrap gap-2">
                    <button
                      type="button"
                      onClick={() => transferClassRepresentative(g.id)}
                      className="inline-flex items-center gap-1.5 rounded-xl border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800"
                    >
                      <ArrowLeftRight className="h-4 w-4" />
                      Ceder mi rol
                    </button>
                    {representativeCount < MAX_CLASS_REPRESENTATIVES && (
                      <button
                        type="button"
                        onClick={() => assignClassRepresentative(g.id)}
                        className="inline-flex items-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-3 py-2 text-sm font-semibold text-white shadow-sm shadow-brand-500/25"
                      >
                        <UserPlus className="h-4 w-4" />
                        Nombrar delegado
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

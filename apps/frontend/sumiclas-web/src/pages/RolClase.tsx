import { ShieldCheck, ArrowLeftRight, UserPlus, LogOut } from 'lucide-react';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses } from '@/lib/colorClasses';
import { roleMeta, canManageDelegados, MAX_DELEGADOS } from '@/lib/roles';

export default function RolClase() {
  const { hijoActual, rolActual, otrosTutoresActual, cederRolDelegado, asignarDelegado, renunciarDelegado } =
    useChildContext();

  if (!hijoActual || !rolActual) {
    return (
      <div className="rounded-2xl border border-dashed border-slate-300 bg-white p-8 text-center dark:border-slate-700 dark:bg-slate-900">
        <p className="text-sm text-slate-600 dark:text-slate-400">Todavía no tienes ningún hijo/a registrado.</p>
      </div>
    );
  }

  const meta = roleMeta[rolActual];
  const c = badgeColorClasses[meta.color];
  const numDelegados =
    otrosTutoresActual.filter((t) => t.rol === 'delegado').length + (rolActual === 'delegado' ? 1 : 0);
  const puedeGestionar = canManageDelegados(rolActual);
  const participantes = otrosTutoresActual.filter((t) => t.rol === 'participante');
  const delegadosActuales = otrosTutoresActual.filter((t) => t.rol === 'delegado');

  return (
    <div>
      <div className={`grid h-12 w-12 place-items-center rounded-xl ${c.bg} ${c.text}`}>
        <ShieldCheck className="h-6 w-6" />
      </div>
      <h1 className="mt-4 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
        Mi rol en la clase
      </h1>
      <p className="mt-2 max-w-xl text-slate-600 dark:text-slate-400">
        {hijoActual.colegioNombre}, {hijoActual.claseNombre}
      </p>

      <div className="mt-6 rounded-2xl border border-slate-200 bg-white p-5 dark:border-slate-700 dark:bg-slate-900">
        <div className="flex items-center gap-3">
          <span className={`rounded-full px-3 py-1 text-sm font-bold ${c.bg} ${c.text}`}>{meta.label}</span>
          <span className="text-sm text-slate-500 dark:text-slate-400">
            Delegados de esta clase: {numDelegados}/{MAX_DELEGADOS}
          </span>
        </div>
        <p className="mt-3 text-sm text-slate-600 dark:text-slate-400">{meta.description}</p>
        {rolActual === 'delegado' && delegadosActuales.length >= 1 && (
          <button
            type="button"
            onClick={renunciarDelegado}
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
        {numDelegados === 0 ? (
          <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">Esta clase no tiene delegado todavía.</p>
        ) : (
          <ul className="mt-3 space-y-2">
            {rolActual === 'delegado' && (
              <li className="rounded-2xl border border-slate-200 bg-white p-4 text-sm font-semibold text-slate-900 dark:border-slate-700 dark:bg-slate-900 dark:text-white">
                Tú
              </li>
            )}
            {delegadosActuales.map((t) => (
              <li
                key={t.id}
                className="rounded-2xl border border-slate-200 bg-white p-4 text-sm font-semibold text-slate-900 dark:border-slate-700 dark:bg-slate-900 dark:text-white"
              >
                {t.nombre}
              </li>
            ))}
          </ul>
        )}
      </div>

      {puedeGestionar && (
        <div className="mt-6">
          <h2 className="font-display text-lg font-bold text-slate-900 dark:text-white">Gestionar delegados</h2>
          {participantes.length === 0 ? (
            <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
              No hay más familias participantes en esta clase todavía.
            </p>
          ) : (
            <div className="mt-3 space-y-3">
              {participantes.map((t) => (
                <div
                  key={t.id}
                  className="flex flex-col gap-3 rounded-2xl border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900 sm:flex-row sm:items-center sm:justify-between"
                >
                  <p className="text-sm font-semibold text-slate-900 dark:text-white">{t.nombre}</p>
                  <div className="flex flex-wrap gap-2">
                    <button
                      type="button"
                      onClick={() => cederRolDelegado(t.id)}
                      className="inline-flex items-center gap-1.5 rounded-xl border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800"
                    >
                      <ArrowLeftRight className="h-4 w-4" />
                      Ceder mi rol
                    </button>
                    {numDelegados < MAX_DELEGADOS && (
                      <button
                        type="button"
                        onClick={() => asignarDelegado(t.id)}
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

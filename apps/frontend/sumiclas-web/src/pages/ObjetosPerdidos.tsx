import { useMemo, useState } from 'react';
import { PackageSearch, Plus } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { ObjetoCard } from '@/components/app/ObjetoCard';
import { ObjetoDetalleModal } from '@/components/app/ObjetoDetalleModal';
import { ObjetoFormulario, type NuevoObjeto } from '@/components/app/ObjetoFormulario';
import { FiltroEstadoObjetoDropdown, type FiltroEstadoObjeto } from '@/components/app/FiltroEstadoObjetoDropdown';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses } from '@/lib/colorClasses';
import { getTutorInfo } from '@/lib/auth';
import { roleMeta } from '@/lib/roles';
import { objetosPerdidosMock, type ObjetoPerdido } from '@/data/objetosPerdidosMock';

type FiltroAlcance = 'todos' | 'colegio' | 'clase';

const filtrosAlcance: { value: FiltroAlcance; label: string }[] = [
  { value: 'todos', label: 'Todos' },
  { value: 'colegio', label: 'Colegio' },
  { value: 'clase', label: 'Mi clase' },
];

export default function ObjetosPerdidos() {
  const { hijos, hijoActual, rolActual } = useChildContext();

  const [objetosCreados, setObjetosCreados] = useState<ObjetoPerdido[]>([]);
  const [recuperadosToggleados, setRecuperadosToggleados] = useState<Set<string>>(new Set());
  const [filtroAlcance, setFiltroAlcance] = useState<FiltroAlcance>('todos');
  const [filtroEstado, setFiltroEstado] = useState<FiltroEstadoObjeto>('activos');
  const [objetoAbierto, setObjetoAbierto] = useState<ObjetoPerdido | null>(null);
  const [formularioAbierto, setFormularioAbierto] = useState(false);

  const claseNombrePorHijo = useMemo(
    () => Object.fromEntries(hijos.map((h) => [h.id, h.claseNombre])),
    [hijos]
  );

  const estaRecuperado = (objeto: ObjetoPerdido) =>
    recuperadosToggleados.has(objeto.id) ? !objeto.recuperado : objeto.recuperado;

  const objetosVisibles = useMemo(() => {
    const misHijoIds = new Set(hijos.map((h) => h.id));
    return [...objetosPerdidosMock, ...objetosCreados]
      .filter((o) => {
        if (filtroAlcance === 'colegio') return o.alcance === 'colegio';
        if (filtroAlcance === 'clase') return o.alcance === 'clase' && !!o.hijoId && misHijoIds.has(o.hijoId);
        if (o.alcance === 'colegio') return true;
        return !!o.hijoId && misHijoIds.has(o.hijoId);
      })
      .filter((o) => {
        const recuperado = estaRecuperado(o);
        if (filtroEstado === 'recuperado') return recuperado;
        if (filtroEstado === 'buscado') return o.tipo === 'buscado' && !recuperado;
        if (filtroEstado === 'encontrado') return o.tipo === 'encontrado' && !recuperado;
        return !recuperado;
      })
      .sort((a, b) => new Date(b.fecha).getTime() - new Date(a.fecha).getTime());
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hijos, objetosCreados, filtroAlcance, filtroEstado, recuperadosToggleados]);

  const toggleRecuperado = (id: string) =>
    setRecuperadosToggleados((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  const puedeCrear = !!hijoActual;

  const handleCrearObjeto = (data: NuevoObjeto) => {
    if (!hijoActual) return;
    const nuevo: ObjetoPerdido = {
      id: `local-${Date.now()}`,
      alcance: data.alcance,
      hijoId: data.alcance === 'clase' ? hijoActual.id : undefined,
      tipo: data.tipo,
      recuperado: false,
      descripcion: data.descripcion,
      fotoUrl: data.fotoUrl,
      fecha: new Date().toISOString(),
      autorNombre: getTutorInfo()?.nombre ?? 'Tú',
      autorRol: rolActual === 'delegado' ? roleMeta.delegado.label : 'Familia',
    };
    setObjetosCreados((prev) => [...prev, nuevo]);
    setFormularioAbierto(false);
  };

  return (
    <div>
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div
            className={`grid h-12 w-12 place-items-center rounded-xl ${badgeColorClasses.orange.bg} ${badgeColorClasses.orange.text}`}
          >
            <PackageSearch className="h-6 w-6" />
          </div>
          <h1 className="mt-4 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
            Objetos perdidos
          </h1>
          <p className="mt-2 truncate text-slate-600 dark:text-slate-400">
            Tablón de buscado / encontrado, visible para la clase o para todo el colegio.
          </p>
        </div>

        {puedeCrear && (
          <button
            type="button"
            onClick={() => setFormularioAbierto(true)}
            className="inline-flex shrink-0 items-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-3.5 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25"
          >
            <Plus className="h-4 w-4" />
            Nuevo
          </button>
        )}
      </div>

      <div className="mt-6 flex flex-wrap items-center gap-2">
        {filtrosAlcance.map((op) => (
          <button
            key={op.value}
            type="button"
            onClick={() => setFiltroAlcance(op.value)}
            className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
              filtroAlcance === op.value
                ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
            }`}
          >
            {op.label}
          </button>
        ))}

        <span className="mx-0.5 h-5 w-px bg-slate-200 dark:bg-slate-700" aria-hidden="true" />

        <FiltroEstadoObjetoDropdown valor={filtroEstado} onCambiar={setFiltroEstado} />
      </div>

      {objetosVisibles.length === 0 ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          {filtroEstado === 'recuperado' ? 'No hay objetos recuperados todavía.' : 'No hay objetos por ahora.'}
        </div>
      ) : (
        <div className="mt-6 space-y-3">
          {objetosVisibles.map((objeto) => (
            <Reveal key={objeto.id}>
              <ObjetoCard
                objeto={objeto}
                claseNombre={objeto.hijoId ? claseNombrePorHijo[objeto.hijoId] : undefined}
                recuperado={estaRecuperado(objeto)}
                onAbrir={() => setObjetoAbierto(objeto)}
                onToggleRecuperado={() => toggleRecuperado(objeto.id)}
              />
            </Reveal>
          ))}
        </div>
      )}

      {objetoAbierto && (
        <ObjetoDetalleModal
          objeto={objetoAbierto}
          claseNombre={objetoAbierto.hijoId ? claseNombrePorHijo[objetoAbierto.hijoId] : undefined}
          recuperado={estaRecuperado(objetoAbierto)}
          onToggleRecuperado={() => toggleRecuperado(objetoAbierto.id)}
          onClose={() => setObjetoAbierto(null)}
        />
      )}

      {formularioAbierto && hijoActual && (
        <ObjetoFormulario
          claseNombre={hijoActual.claseNombre}
          onSubmit={handleCrearObjeto}
          onClose={() => setFormularioAbierto(false)}
        />
      )}
    </div>
  );
}

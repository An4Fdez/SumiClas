import { useMemo, useState } from 'react';
import { Plus, Wallet } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { GastoCard } from '@/components/app/GastoCard';
import { GastoDetalleModal, type ParticipanteEstado } from '@/components/app/GastoDetalleModal';
import { GastoFormulario, type NuevoGasto } from '@/components/app/GastoFormulario';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses } from '@/lib/colorClasses';
import { getTutorInfo } from '@/lib/auth';
import { roleMeta } from '@/lib/roles';
import { otrosTutoresPorHijo } from '@/data/appMock';
import { eventosCalendarioMock } from '@/data/calendarioMock';
import { gastosClaseMock, type GastoClase } from '@/data/gastosMock';

type FiltroEstado = 'pendientes' | 'pagados' | null;

function nombreParticipante(id: string, hijoId: string): string {
  if (id === 'yo') return getTutorInfo()?.nombre ?? 'Tú';
  return otrosTutoresPorHijo[hijoId]?.find((t) => t.id === id)?.nombre ?? id;
}

export default function Gastos() {
  const { hijos, hijoActual, rolActual, otrosTutoresActual } = useChildContext();

  const [gastosCreados, setGastosCreados] = useState<GastoClase[]>([]);
  const [pagosToggleados, setPagosToggleados] = useState<Set<string>>(new Set());
  const [filtroEstado, setFiltroEstado] = useState<FiltroEstado>(null);
  const [gastoAbierto, setGastoAbierto] = useState<GastoClase | null>(null);
  const [formularioAbierto, setFormularioAbierto] = useState(false);

  const yoHePagado = (gasto: GastoClase) => {
    const base = gasto.pagadoPor.includes('yo');
    return pagosToggleados.has(gasto.id) ? !base : base;
  };

  const contarPagados = (gasto: GastoClase) => {
    const pagados = new Set(gasto.pagadoPor);
    if (gasto.participantesIds.includes('yo')) {
      if (yoHePagado(gasto)) pagados.add('yo');
      else pagados.delete('yo');
    }
    return pagados.size;
  };

  const gastosVisibles = useMemo(() => {
    const misHijoIds = new Set(hijos.map((h) => h.id));
    return [...gastosClaseMock, ...gastosCreados]
      .filter((g) => misHijoIds.has(g.hijoId))
      .filter((g) => {
        if (filtroEstado === 'pendientes') return !yoHePagado(g);
        if (filtroEstado === 'pagados') return yoHePagado(g);
        return true;
      })
      .sort((a, b) => new Date(b.fecha).getTime() - new Date(a.fecha).getTime());
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hijos, gastosCreados, filtroEstado, pagosToggleados]);

  const togglePago = (id: string) =>
    setPagosToggleados((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  const puedeCrear = rolActual === 'delegado' && !!hijoActual;

  const handleCrearGasto = (data: NuevoGasto) => {
    if (!hijoActual) return;
    const evento = data.eventoId ? eventosCalendarioMock.find((ev) => ev.id === data.eventoId) : undefined;
    const nuevo: GastoClase = {
      id: `local-${Date.now()}`,
      hijoId: hijoActual.id,
      concepto: data.concepto,
      importeTotal: data.importeTotal,
      participantesIds: data.participantesIds,
      pagadoPor: [],
      eventoRelacionado: evento ? { id: evento.id, titulo: evento.titulo } : undefined,
      fecha: new Date().toISOString(),
      autorNombre: getTutorInfo()?.nombre ?? 'Tú',
      autorRol: roleMeta.delegado.label,
    };
    setGastosCreados((prev) => [...prev, nuevo]);
    setFormularioAbierto(false);
  };

  const participantesDeGasto = (gasto: GastoClase): ParticipanteEstado[] =>
    gasto.participantesIds.map((id) => ({
      id,
      nombre: nombreParticipante(id, gasto.hijoId),
      pagado: id === 'yo' ? yoHePagado(gasto) : gasto.pagadoPor.includes(id),
      esYo: id === 'yo',
    }));

  return (
    <div>
      <div className="flex items-start justify-between gap-3">
        <div>
          <div
            className={`grid h-12 w-12 place-items-center rounded-xl ${badgeColorClasses.sun.bg} ${badgeColorClasses.sun.text}`}
          >
            <Wallet className="h-6 w-6" />
          </div>
          <h1 className="mt-4 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
            Gastos de clase
          </h1>
          <p className="mt-2 max-w-xl text-slate-600 dark:text-slate-400">
            Llevad la cuenta de los pagos entre todos: regalos, material, excursiones… sin mover ni un euro dentro de la app.
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
        <button
          type="button"
          onClick={() => setFiltroEstado((v) => (v === 'pendientes' ? null : 'pendientes'))}
          className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
            filtroEstado === 'pendientes'
              ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
              : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
          }`}
        >
          Pendientes
        </button>
        <button
          type="button"
          onClick={() => setFiltroEstado((v) => (v === 'pagados' ? null : 'pagados'))}
          className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
            filtroEstado === 'pagados'
              ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
              : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
          }`}
        >
          Pagados
        </button>
      </div>

      {gastosVisibles.length === 0 ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          No hay gastos de clase por ahora.
        </div>
      ) : (
        <div className="mt-6 space-y-3">
          {gastosVisibles.map((gasto) => (
            <Reveal key={gasto.id}>
              <GastoCard
                gasto={gasto}
                yoParticipo={gasto.participantesIds.includes('yo')}
                yoHePagado={yoHePagado(gasto)}
                numPagados={contarPagados(gasto)}
                importePorFamilia={gasto.importeTotal / gasto.participantesIds.length}
                onAbrir={() => setGastoAbierto(gasto)}
                onTogglePago={() => togglePago(gasto.id)}
              />
            </Reveal>
          ))}
        </div>
      )}

      {gastoAbierto && (
        <GastoDetalleModal
          gasto={gastoAbierto}
          importePorFamilia={gastoAbierto.importeTotal / gastoAbierto.participantesIds.length}
          participantes={participantesDeGasto(gastoAbierto)}
          onTogglePago={() => togglePago(gastoAbierto.id)}
          onClose={() => setGastoAbierto(null)}
        />
      )}

      {formularioAbierto && hijoActual && (
        <GastoFormulario
          claseNombre={hijoActual.claseNombre}
          eventos={eventosCalendarioMock}
          participantesDisponibles={[
            { id: 'yo', nombre: getTutorInfo()?.nombre ?? 'Tú' },
            ...otrosTutoresActual.map((t) => ({ id: t.id, nombre: t.nombre })),
          ]}
          onSubmit={handleCrearGasto}
          onClose={() => setFormularioAbierto(false)}
        />
      )}
    </div>
  );
}

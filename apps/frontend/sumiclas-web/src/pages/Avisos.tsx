import { useMemo, useState } from 'react';
import { AlertCircle, Bell, Plus } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { AvisoCard } from '@/components/app/AvisoCard';
import { AvisoDetalleModal } from '@/components/app/AvisoDetalleModal';
import { AvisoFormulario } from '@/components/app/AvisoFormulario';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses } from '@/lib/colorClasses';
import { getTutorInfo } from '@/lib/auth';
import { roleMeta } from '@/lib/roles';
import { avisosClasePorHijo, avisosColegioMock, type Aviso, type AvisoPrioridad } from '@/data/avisosMock';
import { eventosCalendarioMock } from '@/data/calendarioMock';

type FiltroAlcance = 'todos' | 'colegio' | 'clase';
type FiltroLeido = 'no-leidos' | 'leidos' | null;

const filtrosAlcance: { value: FiltroAlcance; label: string }[] = [
  { value: 'todos', label: 'Todos' },
  { value: 'colegio', label: 'Colegio' },
  { value: 'clase', label: 'Mi clase' },
];

const pesoPrioridad: Record<AvisoPrioridad, number> = { alta: 0, normal: 1, baja: 2 };

export default function Avisos() {
  const { hijos, hijoActual, rolActual } = useChildContext();
  const [avisosCreados, setAvisosCreados] = useState<Aviso[]>([]);
  const [leidos, setLeidos] = useState<Set<string>>(new Set());
  const [filtroAlcance, setFiltroAlcance] = useState<FiltroAlcance>('todos');
  const [soloUrgentes, setSoloUrgentes] = useState(false);
  const [filtroLeido, setFiltroLeido] = useState<FiltroLeido>(null);
  const [avisoAbierto, setAvisoAbierto] = useState<Aviso | null>(null);
  const [formularioAbierto, setFormularioAbierto] = useState(false);

  const claseNombrePorHijo = useMemo(
    () => Object.fromEntries(hijos.map((h) => [h.id, h.claseNombre])),
    [hijos]
  );

  const avisosClaseTodos = useMemo(
    () => [...hijos.flatMap((h) => avisosClasePorHijo[h.id] ?? []), ...avisosCreados],
    [hijos, avisosCreados]
  );

  const esLeido = (aviso: Aviso) => (leidos.has(aviso.id) ? !aviso.leido : aviso.leido);

  const avisos = useMemo(() => {
    const combinados =
      filtroAlcance === 'colegio'
        ? avisosColegioMock
        : filtroAlcance === 'clase'
          ? avisosClaseTodos
          : [...avisosColegioMock, ...avisosClaseTodos];

    const filtrados = combinados
      .filter((a) => (soloUrgentes ? a.prioridad === 'alta' : true))
      .filter((a) => {
        if (filtroLeido === 'no-leidos') return !esLeido(a);
        if (filtroLeido === 'leidos') return esLeido(a);
        return true;
      });

    return [...filtrados].sort((a, b) => {
      if (pesoPrioridad[a.prioridad] !== pesoPrioridad[b.prioridad]) {
        return pesoPrioridad[a.prioridad] - pesoPrioridad[b.prioridad];
      }
      return new Date(b.fecha).getTime() - new Date(a.fecha).getTime();
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filtroAlcance, soloUrgentes, filtroLeido, leidos, avisosClaseTodos]);

  const puedeCrear = rolActual === 'delegado' && !!hijoActual;

  const toggleLeido = (id: string) =>
    setLeidos((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });

  const handleCrearAviso = (data: {
    titulo: string;
    cuerpo: string;
    prioridad: AvisoPrioridad;
    eventoId: string | null;
  }) => {
    if (!hijoActual) return;
    const evento = eventosCalendarioMock.find((ev) => ev.id === data.eventoId);
    const nuevo: Aviso = {
      id: `local-${Date.now()}`,
      alcance: 'clase',
      hijoId: hijoActual.id,
      titulo: data.titulo,
      cuerpo: data.cuerpo,
      prioridad: data.prioridad,
      autorNombre: getTutorInfo()?.nombre ?? 'Tú',
      autorRol: roleMeta.delegado.label,
      fecha: new Date().toISOString(),
      eventoRelacionado: evento,
      leido: true,
    };
    setAvisosCreados((prev) => [nuevo, ...prev]);
    setFormularioAbierto(false);
  };

  return (
    <div>
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div
            className={`grid h-12 w-12 place-items-center rounded-xl ${badgeColorClasses.rose.bg} ${badgeColorClasses.rose.text}`}
          >
            <Bell className="h-6 w-6" />
          </div>
          <h1 className="mt-4 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
            Avisos
          </h1>
          <p className="mt-2 truncate text-slate-600 dark:text-slate-400">
            Los avisos del colegio y de tu clase, ordenados por fecha y con lo prioritario destacado arriba.
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

      <div className="mt-6 flex flex-wrap items-center justify-between gap-2">
        <div className="flex flex-wrap items-center gap-2">
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

          <button
            type="button"
            onClick={() => setSoloUrgentes((v) => !v)}
            className={`inline-flex items-center gap-1.5 rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
              soloUrgentes
                ? 'bg-rose-100 text-rose-700 dark:bg-rose-900/40 dark:text-rose-300'
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
            }`}
          >
            <AlertCircle className="h-3.5 w-3.5" />
            Urgentes
          </button>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            onClick={() => setFiltroLeido((v) => (v === 'no-leidos' ? null : 'no-leidos'))}
            className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
              filtroLeido === 'no-leidos'
                ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
            }`}
          >
            No leídos
          </button>

          <button
            type="button"
            onClick={() => setFiltroLeido((v) => (v === 'leidos' ? null : 'leidos'))}
            className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
              filtroLeido === 'leidos'
                ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
            }`}
          >
            Leídos
          </button>
        </div>
      </div>

      {avisos.length === 0 ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          No hay avisos por ahora.
        </div>
      ) : (
        <div className="mt-6 space-y-3">
          {avisos.map((aviso) => (
            <Reveal key={aviso.id}>
              <AvisoCard
                aviso={{ ...aviso, leido: esLeido(aviso) }}
                claseNombre={aviso.hijoId ? claseNombrePorHijo[aviso.hijoId] : undefined}
                onOpen={() => setAvisoAbierto(aviso)}
                onToggleLeido={() => toggleLeido(aviso.id)}
              />
            </Reveal>
          ))}
        </div>
      )}

      {avisoAbierto && (
        <AvisoDetalleModal
          aviso={avisoAbierto}
          claseNombre={avisoAbierto.hijoId ? claseNombrePorHijo[avisoAbierto.hijoId] : undefined}
          onClose={() => setAvisoAbierto(null)}
        />
      )}

      {formularioAbierto && hijoActual && (
        <AvisoFormulario
          claseNombre={hijoActual.claseNombre}
          eventos={eventosCalendarioMock}
          onSubmit={handleCrearAviso}
          onClose={() => setFormularioAbierto(false)}
        />
      )}
    </div>
  );
}

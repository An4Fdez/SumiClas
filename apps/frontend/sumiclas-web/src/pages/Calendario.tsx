import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Calendar, Plus, Repeat } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { CalendarioGrid } from '@/components/app/CalendarioGrid';
import { CalendarioSemanaGrid } from '@/components/app/CalendarioSemanaGrid';
import { CalendarioNavHeader } from '@/components/app/CalendarioNavHeader';
import { CalendarioAgendaDia } from '@/components/app/CalendarioAgendaDia';
import { DiaDetalleModal } from '@/components/app/DiaDetalleModal';
import { EventoDetalleModal } from '@/components/app/EventoDetalleModal';
import { EventoAgendaRow } from '@/components/app/EventoAgendaRow';
import { EventoFormulario, type NuevoEvento } from '@/components/app/EventoFormulario';
import { FiltroTipoEventoDropdown } from '@/components/app/FiltroTipoEventoDropdown';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses } from '@/lib/colorClasses';
import { getTutorInfo } from '@/lib/auth';
import { roleMeta } from '@/lib/roles';
import { anioVisibleEvento, esMismoDiaCalendario, eventoOcurreEnDia, formatDiaCompleto } from '@/lib/date';
import { eventosCalendarioMock, type EventoCalendario, type EventoTipo } from '@/data/calendarioMock';

type Vista = 'dia' | 'semana' | 'mes' | 'lista';
type FiltroAlcance = 'todos' | 'colegio' | 'clase';

const ordenVistas: Vista[] = ['dia', 'semana', 'mes', 'lista'];
const vistaLabels: Record<Vista, string> = { dia: 'Día', semana: 'Semana', mes: 'Mes', lista: 'Lista' };

const filtrosAlcance: { value: FiltroAlcance; label: string }[] = [
  { value: 'todos', label: 'Todos' },
  { value: 'colegio', label: 'Colegio' },
  { value: 'clase', label: 'Mi clase' },
];

export default function Calendario() {
  const { hijos, hijoActual, rolActual } = useChildContext();
  const [searchParams, setSearchParams] = useSearchParams();

  const [vista, setVista] = useState<Vista>('mes');
  const [fechaReferencia, setFechaReferencia] = useState(new Date());
  const [filtroAlcance, setFiltroAlcance] = useState<FiltroAlcance>('todos');
  const [filtrosTipo, setFiltrosTipo] = useState<Set<EventoTipo>>(new Set());
  const [diaModalAbierto, setDiaModalAbierto] = useState<Date | null>(null);
  const [eventoAbierto, setEventoAbierto] = useState<EventoCalendario | null>(null);
  const [formularioAbierto, setFormularioAbierto] = useState(false);
  const [eventosCreados, setEventosCreados] = useState<EventoCalendario[]>([]);

  const claseNombrePorHijo = useMemo(
    () => Object.fromEntries(hijos.map((h) => [h.id, h.claseNombre])),
    [hijos]
  );

  const todosLosEventos = useMemo(
    () => [...eventosCalendarioMock, ...eventosCreados],
    [eventosCreados]
  );

  const eventosVisibles = useMemo(() => {
    const misHijoIds = new Set(hijos.map((h) => h.id));
    return todosLosEventos
      .filter((ev) => {
        if (filtroAlcance === 'colegio') return ev.alcance === 'colegio';
        if (filtroAlcance === 'clase') return ev.alcance === 'clase' && !!ev.hijoId && misHijoIds.has(ev.hijoId);
        if (ev.alcance === 'colegio') return true;
        return !!ev.hijoId && misHijoIds.has(ev.hijoId);
      })
      .filter((ev) => (filtrosTipo.size === 0 ? true : filtrosTipo.has(ev.tipo)));
  }, [todosLosEventos, filtroAlcance, filtrosTipo, hijos]);

  const eventosDelDia = (dia: Date): EventoCalendario[] =>
    eventosVisibles.filter((ev) => eventoOcurreEnDia(ev.fecha, dia, ev.tipo === 'cumpleanos'));

  const claseNombreDeEvento = (evento: EventoCalendario) =>
    evento.hijoId ? claseNombrePorHijo[evento.hijoId] : undefined;

  const gruposLista = useMemo(() => {
    const hoyInicio = new Date();
    hoyInicio.setHours(0, 0, 0, 0);

    const conFechaEfectiva = eventosVisibles.map((ev) => {
      if (ev.tipo === 'cumpleanos') {
        const base = new Date(ev.fecha);
        let candidato = new Date(hoyInicio.getFullYear(), base.getMonth(), base.getDate());
        if (candidato < hoyInicio) candidato = new Date(hoyInicio.getFullYear() + 1, base.getMonth(), base.getDate());
        return { evento: ev, fechaEfectiva: candidato };
      }
      return { evento: ev, fechaEfectiva: new Date(ev.fecha) };
    });

    const futuros = conFechaEfectiva
      .filter(({ fechaEfectiva }) => fechaEfectiva >= hoyInicio)
      .sort((a, b) => a.fechaEfectiva.getTime() - b.fechaEfectiva.getTime());

    const grupos: { fecha: Date; eventos: EventoCalendario[] }[] = [];
    for (const { evento, fechaEfectiva } of futuros) {
      const grupo = grupos.find((g) => esMismoDiaCalendario(g.fecha, fechaEfectiva));
      if (grupo) grupo.eventos.push(evento);
      else grupos.push({ fecha: fechaEfectiva, eventos: [evento] });
    }
    return grupos;
  }, [eventosVisibles]);

  useEffect(() => {
    const eventoId = searchParams.get('evento');
    if (!eventoId) return;
    const evento = todosLosEventos.find((ev) => ev.id === eventoId);
    if (!evento) return;

    const fechaEvento = new Date(evento.fecha);
    const anioObjetivo = anioVisibleEvento(evento.fecha, evento.tipo === 'cumpleanos');

    setVista('mes');
    setFechaReferencia(new Date(anioObjetivo, fechaEvento.getMonth(), fechaEvento.getDate()));
    setEventoAbierto(evento);
    setSearchParams({}, { replace: true });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchParams]);

  const cambiarFecha = (delta: number) => {
    setFechaReferencia((prev) => {
      const next = new Date(prev);
      if (vista === 'dia') next.setDate(prev.getDate() + delta);
      else if (vista === 'semana') next.setDate(prev.getDate() + delta * 7);
      else next.setMonth(prev.getMonth() + delta);
      return next;
    });
  };

  const irAHoy = () => setFechaReferencia(new Date());

  const siguienteVista = () =>
    setVista((v) => ordenVistas[(ordenVistas.indexOf(v) + 1) % ordenVistas.length]);

  const puedeCompleto = rolActual === 'delegado';
  const puedeCrear = !!hijoActual;

  const handleCrearEvento = (data: NuevoEvento) => {
    if (!hijoActual) return;

    let nuevo: EventoCalendario;
    if (data.tipo === 'cumpleanos') {
      const hijo = hijos.find((h) => h.id === data.hijoId) ?? hijoActual;
      nuevo = {
        id: `local-${Date.now()}`,
        alcance: 'clase',
        hijoId: hijo.id,
        tipo: 'cumpleanos',
        titulo: `Cumpleaños de ${hijo.alumnoNombre}`,
        fecha: new Date(1900, data.mes - 1, data.dia).toISOString(),
        autorNombre: getTutorInfo()?.nombre ?? 'Tú',
        autorRol: 'Familia',
      };
    } else if (data.tipo === 'otros') {
      const [anioF, mesF, diaF] = data.fecha.split('-').map(Number);
      const [horaF, minF] = data.hora ? data.hora.split(':').map(Number) : [0, 0];
      const fechaEvento = new Date(anioF, mesF - 1, diaF, horaF, minF);
      nuevo = {
        id: `local-${Date.now()}`,
        alcance: data.alcance,
        hijoId: data.alcance === 'clase' ? hijoActual.id : undefined,
        tipo: 'otros',
        titulo: data.titulo,
        fecha: fechaEvento.toISOString(),
        hora: data.hora || undefined,
        ubicacion: data.ubicacion || undefined,
        coste: data.coste || undefined,
        autorNombre: getTutorInfo()?.nombre ?? 'Tú',
        autorRol: roleMeta.delegado.label,
      };
    } else {
      const [anioF, mesF, diaF] = data.fecha.split('-').map(Number);
      const [horaF, minF] = data.hora ? data.hora.split(':').map(Number) : [0, 0];
      const fechaEvento = new Date(anioF, mesF - 1, diaF, horaF, minF);
      nuevo = {
        id: `local-${Date.now()}`,
        alcance: 'clase',
        hijoId: hijoActual.id,
        tipo: data.tipo,
        titulo: data.titulo,
        fecha: fechaEvento.toISOString(),
        hora: data.hora || undefined,
        ubicacion: data.ubicacion || undefined,
        coste: data.coste || undefined,
        autorNombre: getTutorInfo()?.nombre ?? 'Tú',
        autorRol: roleMeta.delegado.label,
      };
    }

    setEventosCreados((prev) => [...prev, nuevo]);
    setFormularioAbierto(false);

    const anioObjetivo = anioVisibleEvento(nuevo.fecha, nuevo.tipo === 'cumpleanos');
    const fechaNuevo = new Date(nuevo.fecha);
    setVista('mes');
    setFechaReferencia(new Date(anioObjetivo, fechaNuevo.getMonth(), fechaNuevo.getDate()));
  };

  return (
    <div>
      <div className="flex items-start justify-between gap-3">
        <div>
          <div
            className={`grid h-12 w-12 place-items-center rounded-xl ${badgeColorClasses.violet.bg} ${badgeColorClasses.violet.text}`}
          >
            <Calendar className="h-6 w-6" />
          </div>
          <h1 className="mt-4 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
            Calendario
          </h1>
          <p className="mt-2 max-w-xl text-slate-600 dark:text-slate-400">
            Excursiones, reuniones y actividades de la clase, todo en un mismo sitio.
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

          <span className="mx-0.5 h-5 w-px bg-slate-200 dark:bg-slate-700" aria-hidden="true" />

          <FiltroTipoEventoDropdown seleccionados={filtrosTipo} onCambiar={setFiltrosTipo} />
        </div>

        <button
          type="button"
          onClick={siguienteVista}
          className="inline-flex items-center gap-1.5 rounded-full bg-brand-600 px-3.5 py-1.5 text-sm font-semibold text-white transition-colors hover:bg-brand-700"
        >
          <Repeat className="h-3.5 w-3.5" />
          {vistaLabels[vista]}
        </button>
      </div>

      {vista === 'mes' && (
        <Reveal className="mt-6">
          <CalendarioGrid
            anio={fechaReferencia.getFullYear()}
            mes={fechaReferencia.getMonth()}
            eventosDelDia={eventosDelDia}
            onCambiarMes={cambiarFecha}
            onHoy={irAHoy}
            onAbrirDia={setDiaModalAbierto}
          />
        </Reveal>
      )}

      {vista === 'semana' && (
        <Reveal className="mt-6">
          <CalendarioSemanaGrid
            fechaReferencia={fechaReferencia}
            eventosDelDia={eventosDelDia}
            onCambiarSemana={cambiarFecha}
            onHoy={irAHoy}
            onAbrirDia={setDiaModalAbierto}
          />
        </Reveal>
      )}

      {vista === 'dia' && (
        <Reveal className="mt-6">
          <div className="rounded-2xl border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900">
            <CalendarioNavHeader
              titulo={formatDiaCompleto(fechaReferencia)}
              onPrev={() => cambiarFecha(-1)}
              onNext={() => cambiarFecha(1)}
              onHoy={irAHoy}
            />
            <CalendarioAgendaDia
              dia={fechaReferencia}
              eventos={eventosDelDia(fechaReferencia)}
              claseNombreDeEvento={claseNombreDeEvento}
              onAbrirEvento={setEventoAbierto}
              mostrarFecha={false}
            />
          </div>
        </Reveal>
      )}

      {vista === 'lista' && (
        <div className="mt-6">
          {gruposLista.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
              No hay eventos próximos.
            </div>
          ) : (
            <div className="space-y-6">
              {gruposLista.map((grupo) => (
                <Reveal key={grupo.fecha.toISOString()}>
                  <h3 className="text-sm font-bold text-slate-500 dark:text-slate-400">
                    {formatDiaCompleto(grupo.fecha)}
                  </h3>
                  <div className="mt-2 space-y-2">
                    {grupo.eventos.map((evento) => (
                      <EventoAgendaRow
                        key={evento.id}
                        evento={evento}
                        alcanceLabel={evento.alcance === 'colegio' ? 'Colegio' : (claseNombreDeEvento(evento) ?? 'Clase')}
                        onAbrir={() => setEventoAbierto(evento)}
                      />
                    ))}
                  </div>
                </Reveal>
              ))}
            </div>
          )}
        </div>
      )}

      {diaModalAbierto && (
        <DiaDetalleModal
          dia={diaModalAbierto}
          eventos={eventosDelDia(diaModalAbierto)}
          claseNombreDeEvento={claseNombreDeEvento}
          onAbrirEvento={(evento) => {
            setDiaModalAbierto(null);
            setEventoAbierto(evento);
          }}
          onClose={() => setDiaModalAbierto(null)}
        />
      )}

      {eventoAbierto && (
        <EventoDetalleModal
          evento={eventoAbierto}
          claseNombre={claseNombreDeEvento(eventoAbierto)}
          onClose={() => setEventoAbierto(null)}
        />
      )}

      {formularioAbierto && hijoActual && (
        <EventoFormulario
          puedeCompleto={puedeCompleto}
          claseNombre={hijoActual.claseNombre}
          hijos={hijos}
          hijoInicialId={hijoActual.id}
          onSubmit={handleCrearEvento}
          onClose={() => setFormularioAbierto(false)}
        />
      )}
    </div>
  );
}

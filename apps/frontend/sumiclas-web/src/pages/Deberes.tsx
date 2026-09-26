import { useMemo, useState } from 'react';
import { BookOpen, Plus } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { TareaDeberesCard } from '@/components/app/TareaDeberesCard';
import { TareaDetalleModal } from '@/components/app/TareaDetalleModal';
import { TareaFormulario, type NuevaTarea } from '@/components/app/TareaFormulario';
import { FiltroAsignaturaDropdown } from '@/components/app/FiltroAsignaturaDropdown';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses } from '@/lib/colorClasses';
import { getTutorInfo } from '@/lib/auth';
import { roleMeta } from '@/lib/roles';
import { esMismoDiaCalendario, formatDiaCompleto } from '@/lib/date';
import {
  asignaturasDisponiblesMock,
  buscarAsignatura,
  comentariosDeberesMock,
  crearAsignatura,
  tareasDeberesMock,
  type AsignaturaInfo,
  type ComentarioDeberes,
  type TareaDeberes,
} from '@/data/deberesMock';

type FiltroEstado = 'pendientes' | 'hechas' | null;

function etiquetaDia(fecha: Date): string {
  const hoy = new Date();
  hoy.setHours(0, 0, 0, 0);
  const cmp = new Date(fecha);
  cmp.setHours(0, 0, 0, 0);
  const diffDias = Math.round((cmp.getTime() - hoy.getTime()) / 86_400_000);

  if (diffDias === 0) return 'Hoy';
  if (diffDias === 1) return 'Mañana';
  if (diffDias === -1) return 'Ayer';
  if (diffDias < 0) return `Atrasado · ${formatDiaCompleto(fecha)}`;
  return formatDiaCompleto(fecha);
}

export default function Deberes() {
  const { hijos, hijoActual, rolActual } = useChildContext();

  const [tareasCreadas, setTareasCreadas] = useState<TareaDeberes[]>([]);
  const [comentariosCreados, setComentariosCreados] = useState<ComentarioDeberes[]>([]);
  const [asignaturasCreadas, setAsignaturasCreadas] = useState<AsignaturaInfo[]>([]);
  const [hechos, setHechos] = useState<Set<string>>(new Set());
  const [filtrosAsignatura, setFiltrosAsignatura] = useState<Set<string>>(new Set());
  const [filtroEstado, setFiltroEstado] = useState<FiltroEstado>(null);
  const [tareaAbierta, setTareaAbierta] = useState<TareaDeberes | null>(null);
  const [formularioAbierto, setFormularioAbierto] = useState(false);

  const todosLosComentarios = useMemo(
    () => [...comentariosDeberesMock, ...comentariosCreados],
    [comentariosCreados]
  );

  const comentariosDeTarea = (tareaId: string) => todosLosComentarios.filter((c) => c.tareaId === tareaId);

  const todasLasAsignaturas = useMemo(
    () => [...asignaturasDisponiblesMock, ...asignaturasCreadas],
    [asignaturasCreadas]
  );

  const tareasVisibles = useMemo(() => {
    const misHijoIds = new Set(hijos.map((h) => h.id));
    return [...tareasDeberesMock, ...tareasCreadas]
      .filter((t) => misHijoIds.has(t.hijoId))
      .filter((t) => (filtrosAsignatura.size === 0 ? true : filtrosAsignatura.has(t.asignaturaId)))
      .filter((t) => {
        if (filtroEstado === 'pendientes') return !hechos.has(t.id);
        if (filtroEstado === 'hechas') return hechos.has(t.id);
        return true;
      })
      .sort((a, b) => new Date(a.fecha).getTime() - new Date(b.fecha).getTime());
  }, [hijos, tareasCreadas, filtrosAsignatura, filtroEstado, hechos]);

  const gruposPorDia = useMemo(() => {
    const grupos: { fecha: Date; tareas: TareaDeberes[] }[] = [];
    for (const tarea of tareasVisibles) {
      const fecha = new Date(tarea.fecha);
      const grupo = grupos.find((g) => esMismoDiaCalendario(g.fecha, fecha));
      if (grupo) grupo.tareas.push(tarea);
      else grupos.push({ fecha, tareas: [tarea] });
    }
    return grupos;
  }, [tareasVisibles]);

  const toggleHecho = (id: string) =>
    setHechos((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  const puedeCompleto = rolActual === 'delegado';
  const puedeCrear = !!hijoActual;

  const handleCrearTarea = (data: NuevaTarea) => {
    if (!hijoActual) return;
    const nueva: TareaDeberes = {
      id: `local-${Date.now()}`,
      hijoId: hijoActual.id,
      asignaturaId: data.asignaturaId,
      titulo: data.titulo,
      descripcion: data.descripcion || undefined,
      fecha: new Date(data.fecha).toISOString(),
      autorNombre: getTutorInfo()?.nombre ?? 'Tú',
      autorRol: puedeCompleto ? roleMeta.delegado.label : 'Familia',
      esPropuesta: !puedeCompleto,
    };
    setTareasCreadas((prev) => [...prev, nueva]);
    setFormularioAbierto(false);
  };

  const handleCrearAsignatura = (nombre: string): AsignaturaInfo => {
    const nueva = crearAsignatura(nombre, todasLasAsignaturas);
    if (!todasLasAsignaturas.some((a) => a.id === nueva.id)) {
      setAsignaturasCreadas((prev) => [...prev, nueva]);
    }
    return nueva;
  };

  const handleAddComentario = (texto: string) => {
    if (!tareaAbierta) return;
    const nuevo: ComentarioDeberes = {
      id: `local-${Date.now()}`,
      tareaId: tareaAbierta.id,
      autorNombre: getTutorInfo()?.nombre ?? 'Tú',
      autorRol: puedeCompleto ? roleMeta.delegado.label : 'Familia',
      texto,
      fecha: new Date().toISOString(),
    };
    setComentariosCreados((prev) => [...prev, nuevo]);
  };

  return (
    <div>
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div
            className={`grid h-12 w-12 place-items-center rounded-xl ${badgeColorClasses.mint.bg} ${badgeColorClasses.mint.text}`}
          >
            <BookOpen className="h-6 w-6" />
          </div>
          <h1 className="mt-4 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
            Deberes
          </h1>
          <p className="mt-2 truncate text-slate-600 dark:text-slate-400">
            Pizarra organizada por día y asignatura, con hilo corto de comentarios para dudas puntuales.
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
        <FiltroAsignaturaDropdown
          asignaturas={todasLasAsignaturas}
          seleccionadas={filtrosAsignatura}
          onCambiar={setFiltrosAsignatura}
        />

        <div className="flex flex-wrap items-center gap-2">
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
            onClick={() => setFiltroEstado((v) => (v === 'hechas' ? null : 'hechas'))}
            className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
              filtroEstado === 'hechas'
                ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
            }`}
          >
            Hechas
          </button>
        </div>
      </div>

      {gruposPorDia.length === 0 ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          No hay deberes pendientes.
        </div>
      ) : (
        <div className="mt-6 space-y-6">
          {gruposPorDia.map((grupo) => (
            <Reveal key={grupo.fecha.toISOString()}>
              <h3 className="text-sm font-bold text-slate-500 dark:text-slate-400">{etiquetaDia(grupo.fecha)}</h3>
              <div className="mt-2 space-y-2">
                {grupo.tareas.map((tarea) => (
                  <TareaDeberesCard
                    key={tarea.id}
                    tarea={tarea}
                    asignatura={buscarAsignatura(tarea.asignaturaId, todasLasAsignaturas)}
                    hecha={hechos.has(tarea.id)}
                    numComentarios={comentariosDeTarea(tarea.id).length}
                    onAbrir={() => setTareaAbierta(tarea)}
                    onToggleHecha={() => toggleHecho(tarea.id)}
                  />
                ))}
              </div>
            </Reveal>
          ))}
        </div>
      )}

      {tareaAbierta && (
        <TareaDetalleModal
          tarea={tareaAbierta}
          asignatura={buscarAsignatura(tareaAbierta.asignaturaId, todasLasAsignaturas)}
          comentarios={comentariosDeTarea(tareaAbierta.id)}
          hecha={hechos.has(tareaAbierta.id)}
          onToggleHecha={() => toggleHecho(tareaAbierta.id)}
          onAddComentario={handleAddComentario}
          onClose={() => setTareaAbierta(null)}
        />
      )}

      {formularioAbierto && hijoActual && (
        <TareaFormulario
          puedeCompleto={puedeCompleto}
          claseNombre={hijoActual.claseNombre}
          asignaturas={todasLasAsignaturas}
          onCrearAsignatura={handleCrearAsignatura}
          onSubmit={handleCrearTarea}
          onClose={() => setFormularioAbierto(false)}
        />
      )}
    </div>
  );
}

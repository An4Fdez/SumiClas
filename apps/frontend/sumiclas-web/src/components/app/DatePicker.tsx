import { useEffect, useLayoutEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { Calendar as CalendarIcon } from 'lucide-react';
import { CalendarioNavHeader } from './CalendarioNavHeader';
import { esMismoDiaCalendario, formatMesAnio, generarMatrizMes } from '@/lib/date';

const diasSemana = ['L', 'M', 'X', 'J', 'V', 'S', 'D'];

function toISODate(d: Date): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}

function fromISODate(s: string): Date | null {
  if (!s) return null;
  const [y, m, d] = s.split('-').map(Number);
  if (!y || !m || !d) return null;
  return new Date(y, m - 1, d);
}

function formatEtiqueta(d: Date): string {
  const dd = String(d.getDate()).padStart(2, '0');
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  return `${dd}/${mm}/${d.getFullYear()}`;
}

export function DatePicker({ value, onChange }: { value: string; onChange: (value: string) => void }) {
  const [open, setOpen] = useState(false);
  const [coords, setCoords] = useState({ top: 0, left: 0, width: 0 });
  const [mesReferencia, setMesReferencia] = useState(() => fromISODate(value) ?? new Date());
  const triggerRef = useRef<HTMLButtonElement>(null);
  const panelRef = useRef<HTMLDivElement>(null);

  const seleccionada = fromISODate(value);

  useLayoutEffect(() => {
    if (!open || !triggerRef.current) return;
    setMesReferencia(seleccionada ?? new Date());
    const rect = triggerRef.current.getBoundingClientRect();
    setCoords({ top: rect.bottom + 8, left: rect.left, width: Math.max(rect.width, 280) });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  useEffect(() => {
    if (!open) return;
    const handlePointerDown = (e: MouseEvent) => {
      const target = e.target as Node;
      if (triggerRef.current?.contains(target) || panelRef.current?.contains(target)) return;
      setOpen(false);
    };
    const handleScroll = (e: Event) => {
      if (panelRef.current?.contains(e.target as Node)) return;
      setOpen(false);
    };
    const handleResize = () => setOpen(false);
    document.addEventListener('mousedown', handlePointerDown);
    window.addEventListener('scroll', handleScroll, true);
    window.addEventListener('resize', handleResize);
    return () => {
      document.removeEventListener('mousedown', handlePointerDown);
      window.removeEventListener('scroll', handleScroll, true);
      window.removeEventListener('resize', handleResize);
    };
  }, [open]);

  const semanas = generarMatrizMes(mesReferencia.getFullYear(), mesReferencia.getMonth());

  return (
    <>
      <button
        ref={triggerRef}
        type="button"
        onClick={() => setOpen((o) => !o)}
        className={`flex w-full items-center justify-between gap-1.5 rounded-xl border border-slate-200 bg-white px-3 py-2 text-left text-sm text-slate-900 transition-colors focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white ${
          !seleccionada ? 'text-slate-400 dark:text-slate-500' : ''
        }`}
      >
        <span>{seleccionada ? formatEtiqueta(seleccionada) : 'dd/mm/aaaa'}</span>
        <CalendarIcon className="h-3.5 w-3.5 shrink-0 text-slate-400" />
      </button>

      {open &&
        createPortal(
          <div
            ref={panelRef}
            style={{ position: 'fixed', top: coords.top, left: coords.left, width: coords.width }}
            className="z-[60] rounded-2xl border border-slate-200 bg-white p-3 shadow-xl dark:border-slate-700 dark:bg-slate-900"
          >
            <CalendarioNavHeader
              titulo={formatMesAnio(mesReferencia.getFullYear(), mesReferencia.getMonth())}
              onPrev={() => setMesReferencia((prev) => new Date(prev.getFullYear(), prev.getMonth() - 1, 1))}
              onNext={() => setMesReferencia((prev) => new Date(prev.getFullYear(), prev.getMonth() + 1, 1))}
              onHoy={() => setMesReferencia(new Date())}
            />

            <div className="mt-3 grid grid-cols-7 gap-1 text-center text-[11px] font-semibold text-slate-400 dark:text-slate-500">
              {diasSemana.map((d) => (
                <div key={d}>{d}</div>
              ))}
            </div>

            <div className="mt-1 grid grid-cols-7 gap-1">
              {semanas.flatMap((semana, si) =>
                semana.map((dia, di) => {
                  const esSeleccionado = !!seleccionada && esMismoDiaCalendario(dia.fecha, seleccionada);
                  return (
                    <button
                      key={`${si}-${di}`}
                      type="button"
                      onClick={() => {
                        onChange(toISODate(dia.fecha));
                        setOpen(false);
                      }}
                      className={`grid h-8 place-items-center rounded-lg text-xs font-semibold transition-colors ${
                        !dia.enMesActual ? 'opacity-40' : ''
                      } ${
                        esSeleccionado
                          ? 'bg-brand-600 text-white'
                          : dia.esHoy
                            ? 'bg-brand-50 text-brand-700 dark:bg-brand-900/20 dark:text-brand-300'
                            : 'text-slate-700 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800'
                      }`}
                    >
                      {dia.fecha.getDate()}
                    </button>
                  );
                })
              )}
            </div>
          </div>,
          document.body
        )}
    </>
  );
}

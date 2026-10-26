import { useLayoutEffect, useRef, useState } from 'react';
import { Clock } from 'lucide-react';
import { FloatingPanel } from './FloatingPanel';

function parseHora(value: string): { h: number; m: number } | null {
  if (!value) return null;
  const [h, m] = value.split(':').map(Number);
  if (Number.isNaN(h) || Number.isNaN(m)) return null;
  return { h, m };
}

function formatHora(h: number, m: number): string {
  return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`;
}

const horas = Array.from({ length: 24 }, (_, i) => i);
const minutos = Array.from({ length: 60 }, (_, i) => i);
const PANEL_MIN_WIDTH = 180;

export function TimePicker({ value, onChange }: { value: string; onChange: (value: string) => void }) {
  const [open, setOpen] = useState(false);
  const triggerRef = useRef<HTMLButtonElement>(null);
  const horaListRef = useRef<HTMLDivElement>(null);
  const minutoListRef = useRef<HTMLDivElement>(null);

  const actual = parseHora(value);

  useLayoutEffect(() => {
    if (!open) return;
    requestAnimationFrame(() => {
      const hIdx = actual?.h ?? 0;
      const mIdx = actual?.m ?? 0;
      (horaListRef.current?.children[hIdx] as HTMLElement | undefined)?.scrollIntoView({ block: 'center' });
      (minutoListRef.current?.children[mIdx] as HTMLElement | undefined)?.scrollIntoView({ block: 'center' });
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  const cambiarHora = (h: number) => onChange(formatHora(h, actual?.m ?? 0));
  const cambiarMinuto = (m: number) => onChange(formatHora(actual?.h ?? 0, m));

  return (
    <>
      <button
        ref={triggerRef}
        type="button"
        onClick={() => setOpen((o) => !o)}
        className={`flex w-full items-center justify-between gap-1.5 rounded-xl border border-slate-200 bg-white px-3 py-2 text-left text-sm text-slate-900 transition-colors focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white ${
          !actual ? 'text-slate-400 dark:text-slate-500' : ''
        }`}
      >
        <span>{actual ? formatHora(actual.h, actual.m) : '--:--'}</span>
        <Clock className="h-3.5 w-3.5 shrink-0 text-slate-400" />
      </button>

      {open && (
        <FloatingPanel
          triggerRef={triggerRef}
          onClose={() => setOpen(false)}
          minWidth={PANEL_MIN_WIDTH}
          className="z-[60] rounded-2xl border border-slate-200 bg-white p-3 shadow-xl dark:border-slate-700 dark:bg-slate-900"
        >
          <div className="grid grid-cols-2 gap-2">
            <div>
              <p className="mb-1 text-center text-[11px] font-semibold text-slate-400 dark:text-slate-500">Hora</p>
              <div ref={horaListRef} className="max-h-40 space-y-0.5 overflow-y-auto">
                {horas.map((h) => (
                  <button
                    key={h}
                    type="button"
                    onClick={() => cambiarHora(h)}
                    className={`block w-full rounded-lg px-2 py-1.5 text-center text-sm font-semibold transition-colors ${
                      actual?.h === h
                        ? 'bg-brand-600 text-white'
                        : 'text-slate-700 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800'
                    }`}
                  >
                    {String(h).padStart(2, '0')}
                  </button>
                ))}
              </div>
            </div>
            <div>
              <p className="mb-1 text-center text-[11px] font-semibold text-slate-400 dark:text-slate-500">Min</p>
              <div ref={minutoListRef} className="max-h-40 space-y-0.5 overflow-y-auto">
                {minutos.map((m) => (
                  <button
                    key={m}
                    type="button"
                    onClick={() => cambiarMinuto(m)}
                    className={`block w-full rounded-lg px-2 py-1.5 text-center text-sm font-semibold transition-colors ${
                      actual?.m === m
                        ? 'bg-brand-600 text-white'
                        : 'text-slate-700 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800'
                    }`}
                  >
                    {String(m).padStart(2, '0')}
                  </button>
                ))}
              </div>
            </div>
          </div>

          <button
            type="button"
            onClick={() => setOpen(false)}
            className="mt-2 w-full rounded-xl bg-brand-600 px-3 py-1.5 text-sm font-semibold text-white transition-colors hover:bg-brand-700"
          >
            Listo
          </button>
        </FloatingPanel>
      )}
    </>
  );
}

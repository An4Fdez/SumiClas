import { useRef, useState, type FormEvent, type MouseEvent } from 'react';
import { X } from 'lucide-react';
import type { ChildView } from '@/context/ChildContext';
import { monthNames } from '@/lib/date';
import { eventTypeMeta, type EventType } from '@/data/calendarMock';
import { SelectDropdown } from './SelectDropdown';
import { DatePicker } from './DatePicker';
import { TimePicker } from './TimePicker';

const allTypes: EventType[] = ['trip', 'meeting', 'activity', 'other', 'birthday'];

export type NewOfficialEvent = {
  type: 'trip' | 'meeting' | 'activity';
  title: string;
  date: string;
  time: string;
  location: string;
  cost: string;
};

export type NewOtherEvent = {
  type: 'other';
  scope: 'class' | 'school';
  title: string;
  date: string;
  time: string;
  location: string;
  cost: string;
};

export type NewBirthday = {
  type: 'birthday';
  childId: string;
  day: number;
  month: number;
};

export type NewEvent = NewOfficialEvent | NewOtherEvent | NewBirthday;

const inputClasses =
  'mt-1.5 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white';

export function EventForm({
  canPublishOfficial,
  className,
  availableChildren,
  initialChildId,
  onSubmit,
  onClose,
}: {
  canPublishOfficial: boolean;
  className: string;
  availableChildren: ChildView[];
  initialChildId: string;
  onSubmit: (data: NewEvent) => void;
  onClose: () => void;
}) {
  const [type, setType] = useState<EventType>(canPublishOfficial ? 'trip' : 'birthday');
  const [title, setTitle] = useState('');
  const [date, setDate] = useState('');
  const [time, setTime] = useState('');
  const [location, setLocation] = useState('');
  const [cost, setCost] = useState('');
  const [scope, setScope] = useState<'class' | 'school'>('class');
  const [childId, setChildId] = useState(initialChildId);
  const today = new Date();
  const [birthdayDay, setBirthdayDay] = useState(today.getDate());
  const [birthdayMonth, setBirthdayMonth] = useState(today.getMonth() + 1);
  const mouseDownOnBackdrop = useRef(false);

  const isBirthday = type === 'birthday';
  const canSubmit = isBirthday ? childId.length > 0 : title.trim().length > 0 && date.length > 0;
  const daysInMonth = new Date(today.getFullYear(), birthdayMonth, 0).getDate();

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!canSubmit) return;
    if (isBirthday) {
      onSubmit({ type: 'birthday', childId, day: birthdayDay, month: birthdayMonth });
    } else if (type === 'other') {
      onSubmit({
        type: 'other',
        scope,
        title: title.trim(),
        date,
        time,
        location: location.trim(),
        cost: cost.trim(),
      });
    } else {
      onSubmit({
        type: type as 'trip' | 'meeting' | 'activity',
        title: title.trim(),
        date,
        time,
        location: location.trim(),
        cost: cost.trim(),
      });
    }
  };

  const handleChangeMonth = (newMonth: number) => {
    setBirthdayMonth(newMonth);
    const max = new Date(today.getFullYear(), newMonth, 0).getDate();
    if (birthdayDay > max) setBirthdayDay(max);
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-end justify-center bg-slate-900/40 p-0 backdrop-blur-sm sm:items-center sm:p-4"
      onMouseDown={(e: MouseEvent) => {
        mouseDownOnBackdrop.current = e.target === e.currentTarget;
      }}
      onClick={(e: MouseEvent) => {
        if (mouseDownOnBackdrop.current && e.target === e.currentTarget) onClose();
      }}
    >
      <form
        onSubmit={handleSubmit}
        onClick={(e) => e.stopPropagation()}
        className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-t-3xl border border-slate-200 bg-white p-6 shadow-xl dark:border-slate-700 dark:bg-slate-900 sm:rounded-3xl"
      >
        <div className="flex items-center justify-between">
          <h2 className="font-display text-xl font-extrabold text-slate-900 dark:text-white">
            {isBirthday ? 'Añadir cumpleaños' : 'Nuevo evento'}
          </h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
            aria-label="Cerrar"
          >
            <X className="h-5 w-5" />
          </button>
        </div>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">
          {isBirthday
            ? `Se mostrará cada año en el calendario de ${className}, sin guardar el año de nacimiento.`
            : type === 'other' && scope === 'school'
              ? 'Se publicará para todo el colegio.'
              : `Se publicará para ${className}.`}
        </p>

        {canPublishOfficial && (
          <fieldset className="mt-5">
            <legend className="text-sm font-semibold text-slate-700 dark:text-slate-300">Tipo</legend>
            <div className="mt-1.5 flex flex-wrap gap-2">
              {allTypes.map((t) => (
                <button
                  key={t}
                  type="button"
                  onClick={() => setType(t)}
                  className={`rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                    type === t
                      ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/20 dark:text-brand-300'
                      : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
                  }`}
                >
                  {eventTypeMeta[t].label}
                </button>
              ))}
            </div>
          </fieldset>
        )}

        {isBirthday ? (
          <>
            {availableChildren.length > 1 && (
              <div className="mt-4">
                <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">Hijo/a</span>
                <div className="mt-1.5">
                  <SelectDropdown
                    value={childId}
                    options={availableChildren.map((c) => ({ value: c.id, label: c.studentName }))}
                    onChange={setChildId}
                  />
                </div>
              </div>
            )}

            <div className="mt-4 grid grid-cols-2 gap-3">
              <div>
                <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">Día</span>
                <div className="mt-1.5">
                  <SelectDropdown
                    value={String(birthdayDay)}
                    options={Array.from({ length: daysInMonth }, (_, i) => i + 1).map((d) => ({
                      value: String(d),
                      label: String(d),
                    }))}
                    onChange={(v) => setBirthdayDay(Number(v))}
                  />
                </div>
              </div>
              <div>
                <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">Mes</span>
                <div className="mt-1.5">
                  <SelectDropdown
                    value={String(birthdayMonth)}
                    options={monthNames.map((name, i) => ({ value: String(i + 1), label: name }))}
                    onChange={(v) => handleChangeMonth(Number(v))}
                  />
                </div>
              </div>
            </div>
            <p className="mt-2 text-xs text-slate-500 dark:text-slate-400">
              Nunca se pide ni se guarda el año de nacimiento.
            </p>
          </>
        ) : (
          <>
            {type === 'other' && (
              <fieldset className="mt-4">
                <legend className="text-sm font-semibold text-slate-700 dark:text-slate-300">Alcance</legend>
                <div className="mt-1.5 flex gap-2">
                  <button
                    type="button"
                    onClick={() => setScope('class')}
                    className={`flex-1 rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                      scope === 'class'
                        ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/20 dark:text-brand-300'
                        : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
                    }`}
                  >
                    Su clase
                  </button>
                  <button
                    type="button"
                    onClick={() => setScope('school')}
                    className={`flex-1 rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                      scope === 'school'
                        ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/20 dark:text-brand-300'
                        : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
                    }`}
                  >
                    Todo el colegio
                  </button>
                </div>
              </fieldset>
            )}

            <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
              Título
              <input
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                maxLength={80}
                required
                className={inputClasses}
                placeholder="Ej. Excursión al museo"
              />
            </label>

            <div className="mt-4 grid grid-cols-2 gap-3">
              <div>
                <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">Fecha</span>
                <div className="mt-1.5">
                  <DatePicker value={date} onChange={setDate} />
                </div>
              </div>
              <div>
                <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">Hora (opcional)</span>
                <div className="mt-1.5">
                  <TimePicker value={time} onChange={setTime} />
                </div>
              </div>
            </div>

            <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
              Ubicación (opcional)
              <input
                value={location}
                onChange={(e) => setLocation(e.target.value)}
                className={inputClasses}
                placeholder="Ej. Museo de la Ciudad"
              />
            </label>

            <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
              Coste (opcional)
              <input
                value={cost}
                onChange={(e) => setCost(e.target.value)}
                className={inputClasses}
                placeholder="Ej. 5 € por alumno"
              />
            </label>
          </>
        )}

        <button
          type="submit"
          disabled={!canSubmit}
          className="mt-6 w-full rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 transition-opacity disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isBirthday ? 'Añadir cumpleaños' : 'Publicar evento'}
        </button>
      </form>
    </div>
  );
}

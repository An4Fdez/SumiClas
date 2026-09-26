import { useRef, useState, type ChangeEvent, type FormEvent, type MouseEvent } from 'react';
import { ImagePlus, X } from 'lucide-react';
import { itemStatusMeta, type ItemScope, type ItemStatus } from '@/data/lostAndFoundMock';

export type NewItem = {
  status: ItemStatus;
  scope: ItemScope;
  description: string;
  photoUrl?: string;
};

const allStatuses: ItemStatus[] = ['lost', 'found'];

const inputClasses =
  'mt-1.5 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-100 dark:border-slate-700 dark:bg-slate-800 dark:text-white';

export function ItemForm({
  className,
  onSubmit,
  onClose,
}: {
  className: string;
  onSubmit: (data: NewItem) => void;
  onClose: () => void;
}) {
  const [status, setStatus] = useState<ItemStatus>('lost');
  const [scope, setScope] = useState<ItemScope>('class');
  const [description, setDescription] = useState('');
  const [photoUrl, setPhotoUrl] = useState<string | null>(null);
  const [photoConfirmed, setPhotoConfirmed] = useState(false);
  const mouseDownOnBackdrop = useRef(false);

  const canSubmit = description.trim().length > 0 && (!photoUrl || photoConfirmed);

  const handlePhotoChange = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const url = URL.createObjectURL(file);
    setPhotoUrl((prev) => {
      if (prev) URL.revokeObjectURL(prev);
      return url;
    });
    setPhotoConfirmed(false);
    e.target.value = '';
  };

  const removePhoto = () => {
    if (photoUrl) URL.revokeObjectURL(photoUrl);
    setPhotoUrl(null);
    setPhotoConfirmed(false);
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!canSubmit) return;
    onSubmit({ status, scope, description: description.trim(), photoUrl: photoUrl ?? undefined });
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
            Nueva entrada
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
          {scope === 'school' ? 'Se publicará para todo el colegio.' : `Se publicará para ${className}.`}
        </p>

        <fieldset className="mt-5">
          <legend className="text-sm font-semibold text-slate-700 dark:text-slate-300">Tipo</legend>
          <div className="mt-1.5 flex gap-2">
            {allStatuses.map((s) => (
              <button
                key={s}
                type="button"
                onClick={() => setStatus(s)}
                className={`flex-1 rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                  status === s
                    ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/20 dark:text-brand-300'
                    : 'border-slate-200 text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800'
                }`}
              >
                {itemStatusMeta[s].label}
              </button>
            ))}
          </div>
        </fieldset>

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

        <label className="mt-4 block text-sm font-semibold text-slate-700 dark:text-slate-300">
          Descripción
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={3}
            required
            className={`${inputClasses} resize-none`}
            placeholder="Ej. Chaqueta azul marino talla 8, patio"
          />
        </label>

        <div className="mt-4">
          <span className="block text-sm font-semibold text-slate-700 dark:text-slate-300">Foto (opcional)</span>

          {photoUrl ? (
            <div className="mt-1.5 flex items-start gap-3">
              <img src={photoUrl} alt="" className="h-20 w-20 rounded-xl object-cover" />
              <div className="flex-1">
                <label className="flex items-start gap-2 text-xs font-semibold text-slate-600 dark:text-slate-300">
                  <input
                    type="checkbox"
                    checked={photoConfirmed}
                    onChange={(e) => setPhotoConfirmed(e.target.checked)}
                    className="mt-0.5 h-4 w-4 shrink-0 rounded border-slate-300 text-brand-600 focus:ring-brand-400 dark:border-slate-600"
                  />
                  Confirmo que esta foto no muestra a ningún menor.
                </label>
                <button
                  type="button"
                  onClick={removePhoto}
                  className="mt-2 inline-flex items-center gap-1 text-xs font-semibold text-rose-600 hover:text-rose-700 dark:text-rose-400"
                >
                  <X className="h-3.5 w-3.5" />
                  Quitar foto
                </button>
              </div>
            </div>
          ) : (
            <label className="mt-1.5 inline-flex cursor-pointer items-center gap-1.5 rounded-xl border border-dashed border-slate-300 px-3 py-2 text-sm font-semibold text-slate-500 transition-colors hover:bg-slate-50 dark:border-slate-600 dark:text-slate-400 dark:hover:bg-slate-800">
              <ImagePlus className="h-4 w-4" />
              Añadir foto
              <input type="file" accept="image/*" onChange={handlePhotoChange} className="hidden" />
            </label>
          )}
        </div>

        <button
          type="submit"
          disabled={!canSubmit}
          className="mt-6 w-full rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 transition-opacity disabled:cursor-not-allowed disabled:opacity-50"
        >
          Publicar
        </button>
      </form>
    </div>
  );
}

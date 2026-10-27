import { useMemo, useState } from 'react';
import { PackageSearch, Plus } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { ItemCard } from '@/components/app/ItemCard';
import { ItemDetailModal } from '@/components/app/ItemDetailModal';
import { ItemForm, type NewItem } from '@/components/app/ItemForm';
import { ItemStatusFilterDropdown, type ItemStatusFilter } from '@/components/app/ItemStatusFilterDropdown';
import { useChildContext } from '@/hooks/useChildContext';
import { useClassLostItems } from '@/hooks/useClassLostItems';
import { useLostItemRepository } from '@/hooks/useLostItemRepository';
import { useWizardIds } from '@/hooks/useWizardIds';
import { getApiErrorStatus, UserFacingError } from '@/lib/apiError';
import { badgeColorClasses } from '@/lib/colorClasses';
import type { LostItem } from '@/modules/lostItems/domain/LostItem';

type ScopeFilter = 'all' | 'school' | 'class';

const scopeFilters: { value: ScopeFilter; label: string }[] = [
  { value: 'all', label: 'Todos' },
  { value: 'class', label: 'Mi clase' },
];

const TRANSIENT_CLIENT_ERRORS = [408, 429];

export default function LostAndFound() {
  const { currentChild, currentUserId } = useChildContext();
  const classId = currentChild?.classId;
  const repository = useLostItemRepository();
  const ids = useWizardIds();
  const { state, reload, loadMore, loadingMore, loadMoreError, setRecovered, toggleError, patchItem } = useClassLostItems(classId);

  const [scopeFilter, setScopeFilter] = useState<ScopeFilter>('all');
  const [statusFilter, setStatusFilter] = useState<ItemStatusFilter>('active');
  const [openItemId, setOpenItemId] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [editingItemId, setEditingItemId] = useState<string | null>(null);

  const loadedItems = useMemo(() => (state.status === 'ready' ? state.items : []), [state]);
  const hasMore = state.status === 'ready' && !!state.nextCursor;
  const openItem = loadedItems.find((i) => i.id === openItemId) ?? null;
  const editingItem = loadedItems.find((i) => i.id === editingItemId) ?? null;

  const visibleItems = useMemo(
    () =>
      loadedItems
        .filter((i) => scopeFilter === 'all' || i.scope === scopeFilter)
        .filter((i) => {
          if (statusFilter === 'recovered') return i.recovered;
          if (statusFilter === 'lost') return i.status === 'lost' && !i.recovered;
          if (statusFilter === 'found') return i.status === 'found' && !i.recovered;
          return !i.recovered;
        }),
    [loadedItems, scopeFilter, statusFilter],
  );

  const canCreate = !!classId;

  const handleCreateItem = async (data: NewItem) => {
    if (!classId) return;
    let id: string;
    try {
      id = ids.take('lostItem');
    } catch {
      throw new UserFacingError('Tu navegador no permite crear entradas fuera de una conexión segura (HTTPS).');
    }
    try {
      await repository.create({
        id,
        scope: 'class',
        classId,
        status: data.status,
        description: data.description,
        photoPrivacyConfirmed: false,
      });
    } catch (error) {
      const status = getApiErrorStatus(error);
      if (status !== undefined && status < 500 && !TRANSIENT_CLIENT_ERRORS.includes(status)) ids.consume('lostItem');
      if (status === 409) {
        reload();
        throw new UserFacingError('Es posible que la entrada ya se haya creado. Revisa la lista antes de crear otra.');
      }
      throw error;
    }
    ids.consume('lostItem');
    setFormOpen(false);
    reload();
  };

  const handleUpdateItem = async (data: NewItem) => {
    if (!editingItem) return;
    await repository.updateDescription(editingItem.id, data.description);
    setEditingItemId(null);
    reload();
  };

  const handleDeleteItem = async (item: LostItem) => {
    await repository.remove(item.id);
    setOpenItemId(null);
    reload();
  };

  const emptyMessage = hasMore
    ? 'Ningún objeto de los cargados coincide con los filtros. Pulsa «Cargar más» para buscar en más.'
    : statusFilter === 'recovered'
      ? 'No hay objetos recuperados todavía.'
      : 'No hay objetos por ahora.';

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

        {canCreate && (
          <button
            type="button"
            onClick={() => setFormOpen(true)}
            className="inline-flex shrink-0 items-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-3.5 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25"
          >
            <Plus className="h-4 w-4" />
            Nuevo
          </button>
        )}
      </div>

      <div className="mt-6 flex flex-wrap items-center gap-2">
        {scopeFilters.map((op) => (
          <button
            key={op.value}
            type="button"
            onClick={() => setScopeFilter(op.value)}
            className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
              scopeFilter === op.value
                ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
            }`}
          >
            {op.label}
          </button>
        ))}

        <span className="mx-0.5 h-5 w-px bg-slate-200 dark:bg-slate-700" aria-hidden="true" />

        <ItemStatusFilterDropdown value={statusFilter} onChange={setStatusFilter} />
      </div>

      {toggleError && (
        <p role="alert" className="mt-4 text-sm font-semibold text-rose-600 dark:text-rose-400">
          {toggleError}
        </p>
      )}

      {!classId ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          Tu hijo/a todavía no está en ninguna clase, así que no hay objetos que mostrar.
        </div>
      ) : state.status === 'loading' || state.status === 'idle' ? (
        <p className="mt-8 text-sm text-slate-500 dark:text-slate-400">Cargando los objetos…</p>
      ) : state.status === 'error' ? (
        <div role="alert" className="mt-8 rounded-2xl border border-rose-200 bg-rose-50/60 p-4 dark:border-rose-700/40 dark:bg-rose-900/15">
          <p className="text-sm text-slate-700 dark:text-slate-300">{state.message}</p>
          <button
            type="button"
            onClick={reload}
            className="mt-3 text-sm font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
          >
            Reintentar
          </button>
        </div>
      ) : visibleItems.length === 0 ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          {emptyMessage}
        </div>
      ) : (
        <div className="mt-6 space-y-3">
          {visibleItems.map((item) => (
            <Reveal key={item.id}>
              <ItemCard
                item={item}
                className={item.classId ? currentChild?.className : undefined}
                recovered={item.recovered}
                canToggleRecovered={item.authorId === currentUserId}
                onOpen={() => setOpenItemId(item.id)}
                onToggleRecovered={() => setRecovered(item.id, !item.recovered)}
              />
            </Reveal>
          ))}
        </div>
      )}

      {hasMore && (
        <div className="mt-6 text-center">
          <button
            type="button"
            onClick={loadMore}
            disabled={loadingMore}
            className="rounded-full bg-slate-100 px-5 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-200 disabled:opacity-60 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700"
          >
            {loadingMore ? 'Cargando…' : 'Cargar más'}
          </button>
          {loadMoreError && (
            <p role="alert" className="mt-2 text-xs font-semibold text-rose-600 dark:text-rose-400">
              {loadMoreError}
            </p>
          )}
        </div>
      )}

      {openItem && (
        <ItemDetailModal
          item={openItem}
          className={openItem.classId ? currentChild?.className : undefined}
          recovered={openItem.recovered}
          canToggleRecovered={openItem.authorId === currentUserId}
          onToggleRecovered={() => setRecovered(openItem.id, !openItem.recovered)}
          onClose={() => setOpenItemId(null)}
          onEdit={() => {
            setEditingItemId(openItem.id);
            setOpenItemId(null);
          }}
          onDelete={() => handleDeleteItem(openItem)}
          onClaimsChanged={(patch) => patchItem(openItem.id, patch)}
        />
      )}

      {editingItem && currentChild && (
        <ItemForm
          className={currentChild.className}
          item={editingItem}
          onSubmit={handleUpdateItem}
          onClose={() => setEditingItemId(null)}
        />
      )}

      {formOpen && currentChild && (
        <ItemForm className={currentChild.className} onSubmit={handleCreateItem} onClose={() => setFormOpen(false)} />
      )}
    </div>
  );
}

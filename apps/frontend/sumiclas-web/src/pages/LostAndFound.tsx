import { useMemo, useState } from 'react';
import { PackageSearch, Plus } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { ItemCard } from '@/components/app/ItemCard';
import { ItemDetailModal } from '@/components/app/ItemDetailModal';
import { ItemForm, type NewItem } from '@/components/app/ItemForm';
import { ItemStatusFilterDropdown, type ItemStatusFilter } from '@/components/app/ItemStatusFilterDropdown';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses } from '@/lib/colorClasses';
import { getGuardianInfo } from '@/lib/auth';
import { roleMeta } from '@/lib/roles';
import { lostAndFoundMock, type LostItem } from '@/data/lostAndFoundMock';

type ScopeFilter = 'all' | 'school' | 'class';

const scopeFilters: { value: ScopeFilter; label: string }[] = [
  { value: 'all', label: 'Todos' },
  { value: 'school', label: 'Colegio' },
  { value: 'class', label: 'Mi clase' },
];

export default function LostAndFound() {
  const { children, currentChild, currentRole } = useChildContext();

  const [createdItems, setCreatedItems] = useState<LostItem[]>([]);
  const [toggledRecovered, setToggledRecovered] = useState<Set<string>>(new Set());
  const [scopeFilter, setScopeFilter] = useState<ScopeFilter>('all');
  const [statusFilter, setStatusFilter] = useState<ItemStatusFilter>('active');
  const [openItem, setOpenItem] = useState<LostItem | null>(null);
  const [formOpen, setFormOpen] = useState(false);

  const classNameByChild = useMemo(
    () => Object.fromEntries(children.map((c) => [c.id, c.className])),
    [children]
  );

  const isRecovered = (item: LostItem) =>
    toggledRecovered.has(item.id) ? !item.recovered : item.recovered;

  const visibleItems = useMemo(() => {
    const myChildIds = new Set(children.map((c) => c.id));
    return [...lostAndFoundMock, ...createdItems]
      .filter((i) => {
        if (scopeFilter === 'school') return i.scope === 'school';
        if (scopeFilter === 'class') return i.scope === 'class' && !!i.childId && myChildIds.has(i.childId);
        if (i.scope === 'school') return true;
        return !!i.childId && myChildIds.has(i.childId);
      })
      .filter((i) => {
        const recovered = isRecovered(i);
        if (statusFilter === 'recovered') return recovered;
        if (statusFilter === 'lost') return i.status === 'lost' && !recovered;
        if (statusFilter === 'found') return i.status === 'found' && !recovered;
        return !recovered;
      })
      .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [children, createdItems, scopeFilter, statusFilter, toggledRecovered]);

  const toggleRecovered = (id: string) =>
    setToggledRecovered((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  const canCreate = !!currentChild;

  const handleCreateItem = (data: NewItem) => {
    if (!currentChild) return;
    const created: LostItem = {
      id: `local-${Date.now()}`,
      scope: data.scope,
      childId: data.scope === 'class' ? currentChild.id : undefined,
      status: data.status,
      recovered: false,
      description: data.description,
      photoUrl: data.photoUrl,
      date: new Date().toISOString(),
      authorName: getGuardianInfo()?.name ?? 'Tú',
      authorRole: currentRole === 'classRepresentative' ? roleMeta.classRepresentative.label : 'Familia',
    };
    setCreatedItems((prev) => [...prev, created]);
    setFormOpen(false);
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

      {visibleItems.length === 0 ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          {statusFilter === 'recovered' ? 'No hay objetos recuperados todavía.' : 'No hay objetos por ahora.'}
        </div>
      ) : (
        <div className="mt-6 space-y-3">
          {visibleItems.map((item) => (
            <Reveal key={item.id}>
              <ItemCard
                item={item}
                className={item.childId ? classNameByChild[item.childId] : undefined}
                recovered={isRecovered(item)}
                onOpen={() => setOpenItem(item)}
                onToggleRecovered={() => toggleRecovered(item.id)}
              />
            </Reveal>
          ))}
        </div>
      )}

      {openItem && (
        <ItemDetailModal
          item={openItem}
          className={openItem.childId ? classNameByChild[openItem.childId] : undefined}
          recovered={isRecovered(openItem)}
          onToggleRecovered={() => toggleRecovered(openItem.id)}
          onClose={() => setOpenItem(null)}
        />
      )}

      {formOpen && currentChild && (
        <ItemForm
          className={currentChild.className}
          onSubmit={handleCreateItem}
          onClose={() => setFormOpen(false)}
        />
      )}
    </div>
  );
}

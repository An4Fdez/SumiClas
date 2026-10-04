import { PackageCheck, Search, type LucideIcon } from 'lucide-react';
import type { PaletteColor } from '@/lib/colorClasses';
import type { ItemStatus } from '@/modules/lostItems/domain/ILostItemRepository';

export const itemStatusMeta: Record<ItemStatus, { label: string; color: PaletteColor; icon: LucideIcon }> = {
  lost: { label: 'Busco', color: 'rose', icon: Search },
  found: { label: 'Encontrado', color: 'mint', icon: PackageCheck },
};

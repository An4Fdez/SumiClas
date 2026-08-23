import { Bell, Calendar, BookOpen, Wallet, PackageSearch, Sparkles } from 'lucide-react';
import type { LucideIcon } from 'lucide-react';

export type FeatureKey = 'avisos' | 'calendario' | 'deberes' | 'gastos' | 'perdidos' | 'resumen';

export const featureMeta: Record<FeatureKey, { icon: LucideIcon; color: string }> = {
  avisos: { icon: Bell, color: 'rose' },
  calendario: { icon: Calendar, color: 'brand' },
  deberes: { icon: BookOpen, color: 'mint' },
  gastos: { icon: Wallet, color: 'sun' },
  perdidos: { icon: PackageSearch, color: 'rose' },
  resumen: { icon: Sparkles, color: 'brand' },
};

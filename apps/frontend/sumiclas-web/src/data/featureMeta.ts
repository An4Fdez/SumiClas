import { Bell, Calendar, BookOpen, Wallet, PackageSearch, Sparkles } from 'lucide-react';
import type { LucideIcon } from 'lucide-react';

export type FeatureKey = 'announcements' | 'calendar' | 'homework' | 'expenses' | 'lostAndFound' | 'summary';

export const featureMeta: Record<FeatureKey, { icon: LucideIcon; color: string }> = {
  announcements: { icon: Bell, color: 'rose' },
  calendar: { icon: Calendar, color: 'violet' },
  homework: { icon: BookOpen, color: 'mint' },
  expenses: { icon: Wallet, color: 'sun' },
  lostAndFound: { icon: PackageSearch, color: 'orange' },
  summary: { icon: Sparkles, color: 'brand' },
};

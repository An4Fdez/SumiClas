import { Bus, Cake, MoreHorizontal, Star, Users, type LucideIcon } from 'lucide-react';
import type { PaletteColor } from '@/lib/colorClasses';
import type { EventType } from '@/modules/calendarEvents/domain/EventType';

export const eventTypeMeta: Record<EventType, { label: string; color: PaletteColor; icon: LucideIcon }> = {
  trip: { label: 'Excursión', color: 'mint', icon: Bus },
  meeting: { label: 'Reunión', color: 'violet', icon: Users },
  activity: { label: 'Actividad', color: 'sun', icon: Star },
  birthday: { label: 'Cumpleaños', color: 'rose', icon: Cake },
  other: { label: 'Otros', color: 'orange', icon: MoreHorizontal },
};

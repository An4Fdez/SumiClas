import type { LostItem } from './LostItem';

export type LostItemPage = {
  items: LostItem[];
  nextCursor?: string;
};

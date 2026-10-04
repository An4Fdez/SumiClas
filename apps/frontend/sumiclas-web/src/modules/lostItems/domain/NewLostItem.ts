import type { ItemScope } from './ItemScope';
import type { ItemStatus } from './ItemStatus';

export type NewLostItem = {
  id: string;
  scope: ItemScope;
  classId?: string;
  status: ItemStatus;
  description: string;
  photoUrl?: string;
  photoPrivacyConfirmed: boolean;
};

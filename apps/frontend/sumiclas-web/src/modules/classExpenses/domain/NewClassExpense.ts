export type NewClassExpense = {
  id: string;
  classId: string;
  item: string;
  totalAmount: number;
  participantIds: string[];
  relatedEventId?: string;
};

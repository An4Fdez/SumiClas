export type WeeklySummary = {
  weekStart: Date;
  weekEnd: Date;
  /** One sentence per line; empty when the week has no news to tell. */
  lines: string[];
  /** When the text was written; undefined when there is none. */
  generatedAt?: Date;
};

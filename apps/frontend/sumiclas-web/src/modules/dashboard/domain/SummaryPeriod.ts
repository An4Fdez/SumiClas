/** The periods the dashboard offers, in days; the API accepts any from 1 to 90. */
export const SUMMARY_PERIODS = [7, 14, 30] as const;

export type SummaryPeriod = (typeof SUMMARY_PERIODS)[number];

export const DEFAULT_SUMMARY_PERIOD: SummaryPeriod = 7;

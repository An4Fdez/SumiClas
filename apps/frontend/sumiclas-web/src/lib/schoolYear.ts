const FIRST_MONTH_OF_NEXT_SCHOOL_YEAR = 6; // zero-based JS month: 6 = July (the previous year ended on 30 June)
const SCHOOL_YEAR_START_MONTH = 8; // zero-based JS month: 8 = September
const SCHOOL_YEAR_START_DAY = 1;
const SCHOOL_YEAR_END_MONTH = 5; // zero-based JS month: 5 = June
const SCHOOL_YEAR_END_DAY = 30;

/**
 * Spanish school year: it starts on 1 September and ends on 30 June of the following calendar year.
 * From July the school year that matters is the one about to start in September.
 */
export function currentSchoolYear(today: Date = new Date()) {
  const startYear =
    today.getMonth() >= FIRST_MONTH_OF_NEXT_SCHOOL_YEAR ? today.getFullYear() : today.getFullYear() - 1;
  return {
    label: `${startYear}-${startYear + 1}`,
    startDate: new Date(startYear, SCHOOL_YEAR_START_MONTH, SCHOOL_YEAR_START_DAY),
    endDate: new Date(startYear + 1, SCHOOL_YEAR_END_MONTH, SCHOOL_YEAR_END_DAY),
  };
}

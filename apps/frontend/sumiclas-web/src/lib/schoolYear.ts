const FIRST_MONTH_OF_NEXT_SCHOOL_YEAR = 6; // zero-based JS month: 6 = July (the previous year ended on 30 June)

/**
 * Spanish school year: it starts on 1 September and ends on 30 June of the following calendar year.
 * From July the school year that matters is the one about to start in September.
 */
export function currentSchoolYear(today: Date = new Date()) {
  const startYear =
    today.getMonth() >= FIRST_MONTH_OF_NEXT_SCHOOL_YEAR ? today.getFullYear() : today.getFullYear() - 1;
  return {
    label: `${startYear}-${startYear + 1}`,
    startDate: new Date(startYear, 8, 1),
    endDate: new Date(startYear + 1, 5, 30),
  };
}

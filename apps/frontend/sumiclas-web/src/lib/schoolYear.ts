/** Spanish school year: it starts on 1 September and ends on 30 June of the following calendar year. */
export function currentSchoolYear(today: Date = new Date()) {
  const startYear = today.getMonth() >= 8 ? today.getFullYear() : today.getFullYear() - 1;
  return {
    label: `${startYear}-${startYear + 1}`,
    startDate: new Date(startYear, 8, 1),
    endDate: new Date(startYear + 1, 5, 30),
  };
}

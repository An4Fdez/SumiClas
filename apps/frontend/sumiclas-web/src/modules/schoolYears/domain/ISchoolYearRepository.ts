export type SchoolYear = {
  id: string;
  label: string;
};

export type NewSchoolYear = {
  id: string;
  schoolId: string;
  label: string;
  startDate: Date;
  endDate: Date;
};

export interface ISchoolYearRepository {
  listActive(schoolId: string): Promise<SchoolYear[]>;
  create(schoolYear: NewSchoolYear): Promise<void>;
}

export type SchoolClass = {
  id: string;
  name: string;
};

export type NewSchoolClass = {
  id: string;
  schoolYearId: string;
  name: string;
};

export interface IClassRepository {
  listBySchoolYear(schoolYearId: string): Promise<SchoolClass[]>;
  create(schoolClass: NewSchoolClass): Promise<void>;
}

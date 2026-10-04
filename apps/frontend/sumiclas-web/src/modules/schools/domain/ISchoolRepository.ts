export type School = {
  id: string;
  name: string;
  address?: string;
};

export type NewSchool = {
  id: string;
  name: string;
  officialCode: string;
  address?: string;
};

export interface ISchoolRepository {
  searchByName(name: string): Promise<School[]>;
  register(school: NewSchool): Promise<void>;
}

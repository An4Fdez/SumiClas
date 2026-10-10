import type { Province } from './Province';

export interface IProvinceRepository {
  listAll(): Promise<Province[]>;
}

import type { Locality } from './Locality';

export interface ILocalityRepository {
  listByProvince(provinceId: string, name?: string): Promise<Locality[]>;
}

import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { IProvinceRepository } from '../domain/IProvinceRepository';
import type { Province } from '../domain/Province';

export class NSwagProvinceRepository implements IProvinceRepository {
  constructor(private readonly client: ApiClient) {}

  async listAll(): Promise<Province[]> {
    const provinces = await this.client.provinceFinderAll();
    return provinces.map((province) => ({ id: province.id, name: province.name }));
  }
}

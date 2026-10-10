import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { ILocalityRepository } from '../domain/ILocalityRepository';
import type { Locality } from '../domain/Locality';

export class NSwagLocalityRepository implements ILocalityRepository {
  constructor(private readonly client: ApiClient) {}

  async listByProvince(provinceId: string, name?: string): Promise<Locality[]> {
    const localities = await this.client.localityFinderByProvince(provinceId, name);
    return localities.map((locality) => ({
      id: locality.id,
      provinceId: locality.provinceId,
      name: locality.name,
    }));
  }
}

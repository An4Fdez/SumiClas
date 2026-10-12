import { reviveApiDates } from './apiDate';
import { API_BASE_URL, authHttp } from './authHttp';
import { GeneratedApiClient } from './GeneratedApiClient';

export class ApiClient extends GeneratedApiClient {
  protected override jsonParseReviver = reviveApiDates;
}

export function createApiClient(): ApiClient {
  return new ApiClient(API_BASE_URL, authHttp);
}

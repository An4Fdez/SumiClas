import type { ApiClient } from '@/modules/Shared/Infrastructure/ApiClient';
import type { Account } from '../domain/Account';
import type { AccountRegistration } from '../domain/AccountRegistration';
import type { Credentials } from '../domain/Credentials';
import type { IAuthenticationRepository } from '../domain/IAuthenticationRepository';

export class NSwagAuthenticationRepository implements IAuthenticationRepository {
  constructor(private readonly client: ApiClient) {}

  async register(registration: AccountRegistration): Promise<void> {
    await this.client.userRegister({
      id: registration.id,
      fullName: registration.fullName,
      email: registration.email,
      password: registration.password,
    });
  }

  async login(credentials: Credentials): Promise<Account> {
    const account = await this.client.sessionCreate({ email: credentials.email, password: credentials.password });
    return { userId: account.userId, fullName: account.fullName };
  }
}

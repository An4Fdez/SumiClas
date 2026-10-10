import type { Account } from './Account';
import type { AccountRegistration } from './AccountRegistration';
import type { Credentials } from './Credentials';

export interface IAuthenticationRepository {
  register(registration: AccountRegistration): Promise<void>;
  login(credentials: Credentials): Promise<Account>;
}

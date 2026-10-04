import type { Profile } from './Profile';
import type { UserRegistration } from './UserRegistration';

export interface IUserRepository {
  register(user: UserRegistration): Promise<void>;
  getCurrent(): Promise<Profile>;
}

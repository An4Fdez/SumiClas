import type { Child } from './Child';

export interface StudentRepository {
  listMine(): Promise<Child[]>;
}

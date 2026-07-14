export type UserRole = 'DOCENTE' | 'REVISOR' | 'ADMIN';

export interface AppUser {
  id: number;
  name: string;
  email: string;
  role: UserRole;
  initials: string;
}
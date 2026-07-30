import { Injectable, computed, signal } from '@angular/core';

export type UserRole =
  | 'ADMIN'
  | 'REVISOR'
  | 'DOCENTE';

export interface AuthUser {
  id: number;
  nombre: string;
  name: string;
  initials: string;
  email: string;
  role: UserRole;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly storageKey = 'sistema_academico_user';

  private readonly currentUserSignal = signal<AuthUser | null>(
    this.getStoredUser()
  );

  currentUser = this.currentUserSignal.asReadonly();

  user = this.currentUser;

  isAuthenticated = computed(() => this.currentUserSignal() !== null);

  userRole = computed(() => this.currentUserSignal()?.role ?? null);

  login(email: string, password: string): boolean {
    const normalizedEmail = email.trim().toLowerCase();
    const normalizedPassword = password.trim();

    if (!normalizedEmail || !normalizedPassword) {
      return false;
    }

    const user = this.resolveMockUser(normalizedEmail);

    if (!user) {
      return false;
    }

    this.clearOldSessionKeys();

    this.currentUserSignal.set(user);
    localStorage.setItem(this.storageKey, JSON.stringify(user));

    return true;
  }

  logout(): void {
    this.currentUserSignal.set(null);
    this.clearOldSessionKeys();
  }

  isLoggedIn(): boolean {
    return this.isAuthenticated();
  }

  getRole(): UserRole | null {
    return this.userRole();
  }

  hasRole(role: UserRole): boolean {
    return this.userRole() === role;
  }

  private resolveMockUser(email: string): AuthUser | null {
    if (email === 'admin@email.com' || email === 'directivo@email.com') {
      return {
        id: 1,
        nombre: 'Administrador / Directivo',
        name: 'Administrador / Directivo',
        initials: 'AD',
        email,
        role: 'ADMIN'
      };
    }

    if (email === 'revisor@email.com') {
      return {
        id: 2,
        nombre: 'Revisor Académico',
        name: 'Revisor Académico',
        initials: 'RA',
        email,
        role: 'REVISOR'
      };
    }

    if (email === 'docente@email.com') {
      return {
        id: 3,
        nombre: 'Docente',
        name: 'Docente',
        initials: 'DO',
        email,
        role: 'DOCENTE'
      };
    }

    return null;
  }

  private getStoredUser(): AuthUser | null {
    const storedUser = localStorage.getItem(this.storageKey);

    if (!storedUser) return null;

    try {
      const parsedUser = JSON.parse(storedUser) as Partial<AuthUser>;

      if (!parsedUser.email || !parsedUser.role) {
        this.clearOldSessionKeys();
        return null;
      }

      return {
        id: parsedUser.id ?? 0,
        nombre: parsedUser.nombre ?? parsedUser.name ?? 'Usuario',
        name: parsedUser.name ?? parsedUser.nombre ?? 'Usuario',
        initials: parsedUser.initials ?? 'US',
        email: parsedUser.email,
        role: parsedUser.role
      };
    } catch {
      this.clearOldSessionKeys();
      return null;
    }
  }

  private clearOldSessionKeys(): void {
    localStorage.removeItem(this.storageKey);
    localStorage.removeItem('isAuthenticated');
    localStorage.removeItem('userState');
  }
}
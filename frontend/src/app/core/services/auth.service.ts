import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';

export type UserRole = 'DOCENTE' | 'REVISOR' | 'ADMIN';

export interface AppUser {
  id: number;
  name: string;
  email: string;
  role: UserRole;
  initials: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly userKey = 'userState';
  private readonly authKey = 'isAuthenticated';

  currentUser = signal<AppUser | null>(this.getStoredUser());

  constructor(private router: Router) {}

  login(email: string, password: string): void {
    let user: AppUser;

    const emailLower = email.toLowerCase();

    if (emailLower.includes('admin')) {
      user = {
        id: 1,
        name: 'Admin User',
        email: '',
        role: 'ADMIN',
        initials: 'AU'
      };
    } else if (emailLower.includes('revisor')) {
      user = {
        id: 2,
        name: 'Revisor Juan',
        email,
        role: 'REVISOR',
        initials: 'RJ'
      };
    } else {
      user = {
        id: 3,
        name: 'Carlos Pérez',
        email,
        role: 'DOCENTE',
        initials: 'CP'
      };
    }

    localStorage.setItem(this.authKey, 'true');
    localStorage.setItem(this.userKey, JSON.stringify(user));

    this.currentUser.set(user);
    this.router.navigate(['/dashboard']);
  }

  logout(): void {
    localStorage.removeItem(this.authKey);
    localStorage.removeItem(this.userKey);

    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return localStorage.getItem(this.authKey) === 'true' && this.currentUser() !== null;
  }

  private getStoredUser(): AppUser | null {
    const data = localStorage.getItem(this.userKey);

    if (!data) {
      return null;
    }

    try {
      return JSON.parse(data) as AppUser;
    } catch {
      localStorage.removeItem(this.userKey);
      return null;
    }
  }
}
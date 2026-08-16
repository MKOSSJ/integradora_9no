import { Injectable, computed, inject, signal } from '@angular/core';

import { HttpClient } from '@angular/common/http';

import { Observable, tap } from 'rxjs';

import { environment } from '../../environments/environments';

import { LoginRequestDto } from '../dto/auth/login-request.dto';
import { LoginResponseDto } from '../dto/auth/login-response.dto';

import { ForgotPasswordDto } from '../dto/auth/forgot-password.dto';
import { VerifyCodeDto } from '../dto/auth/verify-code.dto';
import { ResetPasswordDto } from '../dto/auth/reset-password.dto';

export type UserRole = 'DIRECTIVO' | 'REVISOR' | 'DOCENTE';

export interface AuthUser {
  id: number;
  nombre: string;
  name: string;
  initials: string;
  email: string;
  role: UserRole;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly storageKey = 'sistema_academico_user';

  private readonly accessTokenKey = 'access_token';

  private readonly refreshTokenKey = 'refresh_token';

  private readonly accessTokenExpiresAtKey = 'access_token_expires_at';

  private readonly currentUserSignal = signal<AuthUser | null>(this.getStoredUser());

  currentUser = this.currentUserSignal.asReadonly();

  user = this.currentUser;

  isAuthenticated = computed(() => !!this.getAccessToken());

  userRole = computed(() => this.currentUserSignal()?.role ?? null);

  constructor() {
    if (!this.currentUserSignal() && this.getAccessToken()) {
      this.loadUserFromToken();
    }
  }

  login(email: string, password: string): Observable<LoginResponseDto> {
    const dto: LoginRequestDto = {
      email: email.trim().toLowerCase(),
      password,
    };

    return this.http.post<LoginResponseDto>(`${environment.apiUrl}/api/Auth/login`, dto).pipe(
      tap((response) => {
        if (!response.success) {
          return;
        }

        localStorage.setItem(this.accessTokenKey, response.accessToken);

        localStorage.setItem(this.refreshTokenKey, response.refreshToken);

        localStorage.setItem(this.accessTokenExpiresAtKey, response.accessTokenExpiresAt);

        this.loadUserFromToken();
      }),
    );
  }

  logout(): void {
    this.currentUserSignal.set(null);

    localStorage.removeItem(this.storageKey);

    localStorage.removeItem(this.accessTokenKey);

    localStorage.removeItem(this.refreshTokenKey);

    localStorage.removeItem(this.accessTokenExpiresAtKey);

    localStorage.removeItem('isAuthenticated');

    localStorage.removeItem('userState');
  }

  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }

  getRole(): UserRole | null {
    return this.userRole();
  }

  hasRole(role: UserRole): boolean {
    return this.userRole() === role;
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  private loadUserFromToken(): void {
    const token = this.getAccessToken();

    if (!token) {
      this.currentUserSignal.set(null);

      return;
    }

    try {
      const payload = this.decodeJwtPayload(token);

      const roleValue =
        payload['role'] ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

      const email =
        payload['email'] ??
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];

      const id =
        payload['sub'] ??
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

      const role = this.mapBackendRole(roleValue);

      console.log('JWT payload:', payload);

      console.log('Rol recibido:', roleValue);

      console.log('Rol Front:', role);

      if (!role) {
        console.warn('El JWT no contiene un rol reconocido:', roleValue);

        this.currentUserSignal.set(null);

        return;
      }

      const name =
        payload['name'] ??
        payload['unique_name'] ??
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ??
        email ??
        'Usuario';

      const user: AuthUser = {
        id: Number(id ?? 0),

        nombre: String(name),

        name: String(name),

        initials: this.getInitials(String(name)),

        email: String(email ?? ''),

        role,
      };

      this.currentUserSignal.set(user);

      localStorage.setItem(this.storageKey, JSON.stringify(user));
    } catch (error) {
      console.error('No se pudo leer el JWT:', error);

      this.currentUserSignal.set(null);
    }
  }

  private decodeJwtPayload(token: string): Record<string, any> {
    const parts = token.split('.');

    if (parts.length !== 3) {
      throw new Error('El access token no tiene un formato JWT válido.');
    }

    const base64Url = parts[1];

    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');

    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((char) => '%' + ('00' + char.charCodeAt(0).toString(16)).slice(-2))
        .join(''),
    );

    return JSON.parse(jsonPayload);
  }

  private mapBackendRole(value: unknown): UserRole | null {
    if (value === undefined || value === null) {
      return null;
    }

    const role = String(value).trim().toLowerCase();

    if (role === '1' || role === 'docente') {
      return 'DOCENTE';
    }

    if (role === '2' || role === 'revisor') {
      return 'REVISOR';
    }

    if (role === '3' || role === 'director' || role === 'directivo' || role === 'administrador') {
      return 'DIRECTIVO';
    }

    return null;
  }

  private getInitials(name: string): string {
    const words = name.trim().split(/\s+/).filter(Boolean);

    if (words.length === 0) {
      return 'US';
    }

    if (words.length === 1) {
      return words[0].substring(0, 2).toUpperCase();
    }

    return (words[0].charAt(0) + words[1].charAt(0)).toUpperCase();
  }

  private getStoredUser(): AuthUser | null {
    const storedUser = localStorage.getItem(this.storageKey);

    if (!storedUser) {
      return null;
    }

    try {
      const parsedUser = JSON.parse(storedUser) as Partial<AuthUser>;

      if (!parsedUser.email || !parsedUser.role) {
        return null;
      }

      const validRoles: UserRole[] = ['DIRECTIVO', 'REVISOR', 'DOCENTE'];

      if (!validRoles.includes(parsedUser.role as UserRole)) {
        return null;
      }

      return {
        id: parsedUser.id ?? 0,

        nombre: parsedUser.nombre ?? parsedUser.name ?? 'Usuario',

        name: parsedUser.name ?? parsedUser.nombre ?? 'Usuario',

        initials: parsedUser.initials ?? 'US',

        email: parsedUser.email,

        role: parsedUser.role as UserRole,
      };
    } catch {
      localStorage.removeItem(this.storageKey);

      return null;
    }
  }

  forgotPassword(dto: ForgotPasswordDto): Observable<unknown> {
    return this.http.post(`${environment.apiUrl}/api/Auth/forgot-password`, dto);
  }

  verifyCode(dto: VerifyCodeDto): Observable<unknown> {
    return this.http.post(`${environment.apiUrl}/api/Auth/verify-code`, dto);
  }

  resetPassword(dto: ResetPasswordDto): Observable<unknown> {
    return this.http.post(`${environment.apiUrl}/api/Auth/reset-password`, dto);
  }
}

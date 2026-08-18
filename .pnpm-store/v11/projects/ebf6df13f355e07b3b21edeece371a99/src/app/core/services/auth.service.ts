import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, finalize, map, of, shareReplay, tap, throwError } from 'rxjs';

import { environment } from '../../environments/environments';
import { ForgotPasswordDto } from '../dto/auth/forgot-password.dto';
import { LoginRequestDto } from '../dto/auth/login-request.dto';
import { LoginResponseDto } from '../dto/auth/login-response.dto';
import { ResetPasswordDto } from '../dto/auth/reset-password.dto';
import { VerifyCodeDto } from '../dto/auth/verify-code.dto';

export type UserRole = 'DIRECTIVO' | 'REVISOR' | 'DOCENTE';

export interface AuthUser {
  id: number;
  nombre: string;
  name: string;
  initials: string;
  email: string;
  role: UserRole;
  roles: UserRole[];
}

interface RefreshTokenRequestDto {
  refreshToken: string;
  accessToken?: string;
}

interface MessageResponseDto {
  message: string;
}

type JwtPayload = Record<string, unknown>;

const ROLE_CLAIM = 'role';
const ASP_NET_ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const EMAIL_CLAIM = 'email';
const ASP_NET_EMAIL_CLAIM =
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
const SUBJECT_CLAIM = 'sub';
const ASP_NET_NAME_IDENTIFIER_CLAIM =
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly storageKey = 'sistema_academico_user';
  private readonly accessTokenKey = 'access_token';
  private readonly refreshTokenKey = 'refresh_token';
  private readonly accessTokenExpiresAtKey = 'access_token_expires_at';

  private readonly currentUserSignal = signal<AuthUser | null>(null);
  private refreshRequest$: Observable<string> | null = null;

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly user = this.currentUser;
  readonly isAuthenticated = computed(() => this.currentUserSignal() !== null);
  readonly userRole = computed(() => this.currentUserSignal()?.role ?? null);

  constructor() {
    this.restoreSession();
  }

  login(email: string, password: string): Observable<LoginResponseDto> {
    const dto: LoginRequestDto = {
      email: email.trim().toLowerCase(),
      password,
    };

    return this.http.post<LoginResponseDto>(`${environment.apiUrl}/api/Auth/login`, dto).pipe(
      tap((response) => {
        if (!response.success || response.requiresTwoFactor) {
          return;
        }

        if (!this.persistSession(response)) {
          throw new Error('La sesión recibida no contiene un token o rol válido.');
        }
      }),
    );
  }

  refreshSession(): Observable<string> {
    if (this.refreshRequest$) {
      return this.refreshRequest$;
    }

    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      this.clearSession();
      return throwError(() => new Error('No existe un refresh token disponible.'));
    }

    const request: RefreshTokenRequestDto = {
      refreshToken,
    };

    const accessToken = this.getAccessToken();

    if (accessToken) {
      request.accessToken = accessToken;
    }

    this.refreshRequest$ = this.http
      .post<LoginResponseDto>(`${environment.apiUrl}/api/Auth/refresh-token`, request)
      .pipe(
        map((response) => {
          if (!response.success || !this.persistSession(response)) {
            throw new Error(response.message || 'No fue posible renovar la sesión.');
          }

          return response.accessToken as string;
        }),
        catchError((error: unknown) => {
          this.clearSession();
          return throwError(() => error);
        }),
        finalize(() => {
          this.refreshRequest$ = null;
        }),
        shareReplay({ bufferSize: 1, refCount: false }),
      );

    return this.refreshRequest$;
  }

  ensureValidSession(): Observable<boolean> {
    if (this.isLoggedIn()) {
      return of(true);
    }

    if (!this.getRefreshToken()) {
      this.clearSession();
      return of(false);
    }

    return this.refreshSession().pipe(
      map(() => this.isLoggedIn()),
      catchError(() => of(false)),
    );
  }

  logout(): void {
    this.clearSession();
  }

  isLoggedIn(): boolean {
    return this.currentUserSignal() !== null && !this.isAccessTokenExpired();
  }

  getRole(): UserRole | null {
    return this.userRole();
  }

  hasRole(role: UserRole): boolean {
    return this.currentUserSignal()?.roles.includes(role) ?? false;
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  isAccessTokenExpired(clockSkewSeconds = 0): boolean {
    const token = this.getAccessToken();

    if (!token) {
      return true;
    }

    try {
      const payload = this.decodeJwtPayload(token);
      const expiration = payload['exp'];

      if (typeof expiration !== 'number') {
        return true;
      }

      const currentTimeSeconds = Math.floor(Date.now() / 1000);
      return expiration <= currentTimeSeconds + clockSkewSeconds;
    } catch {
      return true;
    }
  }

  forgotPassword(dto: ForgotPasswordDto): Observable<MessageResponseDto> {
    return this.http.post<MessageResponseDto>(
      `${environment.apiUrl}/api/Auth/forgot-password`,
      dto,
    );
  }

  resetPassword(dto: ResetPasswordDto): Observable<MessageResponseDto> {
    return this.http.post<MessageResponseDto>(
      `${environment.apiUrl}/api/Auth/reset-password`,
      dto,
    );
  }

  // El backend no expone este flujo. Se conserva la firma para consumidores
  // heredados, pero no se realiza una solicitud a un endpoint inexistente.
  verifyCode(dto: VerifyCodeDto): Observable<unknown> {
    void dto;
    return throwError(() => new Error(
      '[FALTA ENDPOINT] La API no expone verificación por código; use el enlace de recuperación.'
    ));
  }

  private restoreSession(): void {
    const token = this.getAccessToken();

    if (!token) {
      this.clearSession();
      return;
    }

    const user = this.userFromToken(token);

    if (!user) {
      this.clearSession();
      return;
    }

    if (this.isAccessTokenExpired() && !this.getRefreshToken()) {
      this.clearSession();
      return;
    }

    this.setCurrentUser(user);
  }

  private persistSession(response: LoginResponseDto): boolean {
    if (!response.accessToken || !response.refreshToken || !response.accessTokenExpiresAt) {
      this.clearSession();
      return false;
    }

    const user = this.userFromToken(response.accessToken);

    if (!user) {
      this.clearSession();
      return false;
    }

    localStorage.setItem(this.accessTokenKey, response.accessToken);
    localStorage.setItem(this.refreshTokenKey, response.refreshToken);
    localStorage.setItem(this.accessTokenExpiresAtKey, response.accessTokenExpiresAt);
    this.setCurrentUser(user);
    return true;
  }

  private userFromToken(token: string): AuthUser | null {
    try {
      const payload = this.decodeJwtPayload(token);
      const roles = this.readRoles(payload);

      if (roles.length === 0) {
        return null;
      }

      const emailValue = payload[EMAIL_CLAIM] ?? payload[ASP_NET_EMAIL_CLAIM];
      const idValue = payload[SUBJECT_CLAIM] ?? payload[ASP_NET_NAME_IDENTIFIER_CLAIM];
      const email = typeof emailValue === 'string' ? emailValue : '';
      const id = typeof idValue === 'string' || typeof idValue === 'number' ? Number(idValue) : 0;
      const nameValue =
        payload['name'] ??
        payload['unique_name'] ??
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
      const name = typeof nameValue === 'string' && nameValue.trim() ? nameValue : email || 'Usuario';

      return {
        id: Number.isFinite(id) ? id : 0,
        nombre: name,
        name,
        initials: this.getInitials(name),
        email,
        role: this.primaryRole(roles),
        roles,
      };
    } catch {
      return null;
    }
  }

  private readRoles(payload: JwtPayload): UserRole[] {
    const claim = payload[ASP_NET_ROLE_CLAIM] ?? payload[ROLE_CLAIM];
    const values = Array.isArray(claim) ? claim : [claim];
    const roles = values
      .map((value) => this.mapBackendRole(value))
      .filter((role): role is UserRole => role !== null);

    return [...new Set(roles)];
  }

  private mapBackendRole(value: unknown): UserRole | null {
    if (typeof value !== 'string') {
      return null;
    }

    switch (value.trim()) {
      case 'Docente':
        return 'DOCENTE';
      case 'Revisor':
        return 'REVISOR';
      case 'Director':
        return 'DIRECTIVO';
      default:
        return null;
    }
  }

  private primaryRole(roles: UserRole[]): UserRole {
    if (roles.includes('DIRECTIVO')) {
      return 'DIRECTIVO';
    }

    if (roles.includes('REVISOR')) {
      return 'REVISOR';
    }

    return 'DOCENTE';
  }

  private decodeJwtPayload(token: string): JwtPayload {
    const parts = token.split('.');

    if (parts.length !== 3) {
      throw new Error('El access token no tiene un formato JWT válido.');
    }

    const base64Url = parts[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const paddedBase64 = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
    const binaryPayload = atob(paddedBase64);
    const bytes = Uint8Array.from(binaryPayload, (character) => character.charCodeAt(0));
    const parsed: unknown = JSON.parse(new TextDecoder().decode(bytes));

    if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) {
      throw new Error('El payload del access token no es válido.');
    }

    return parsed as JwtPayload;
  }

  private setCurrentUser(user: AuthUser): void {
    this.currentUserSignal.set(user);
    localStorage.setItem(this.storageKey, JSON.stringify(user));
  }

  private clearSession(): void {
    this.currentUserSignal.set(null);
    localStorage.removeItem(this.storageKey);
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.accessTokenExpiresAtKey);
    localStorage.removeItem('isAuthenticated');
    localStorage.removeItem('userState');
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
}

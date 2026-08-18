import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, catchError, switchMap, throwError } from 'rxjs';

import { environment } from '../../environments/environments';
import { AuthService } from '../services/auth.service';

const PUBLIC_AUTH_ENDPOINTS = [
  '/api/auth/login',
  '/api/auth/refresh-token',
  '/api/auth/forgot-password',
  '/api/auth/reset-password',
];

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!isApiRequest(request) || isPublicAuthRequest(request)) {
    return next(request);
  }

  const redirectToLogin = (error: unknown): Observable<never> => {
    authService.logout();
    void router.navigateByUrl('/auth/login', { replaceUrl: true });
    return throwError(() => error);
  };

  const sendOnceWithToken = (accessToken: string): ReturnType<typeof next> =>
    next(withBearer(request, accessToken)).pipe(
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          return redirectToLogin(error);
        }

        return throwError(() => error);
      }),
    );

  const refreshAndRetry = (): ReturnType<typeof next> =>
    authService.refreshSession().pipe(
      catchError((error: unknown) => redirectToLogin(error)),
      switchMap((accessToken) => sendOnceWithToken(accessToken)),
    );

  const accessToken = authService.getAccessToken();

  if (!accessToken || authService.isAccessTokenExpired(30)) {
    return refreshAndRetry();
  }

  return next(withBearer(request, accessToken)).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        return refreshAndRetry();
      }

      return throwError(() => error);
    }),
  );
};

function isApiRequest(request: HttpRequest<unknown>): boolean {
  return request.url.toLowerCase().startsWith(environment.apiUrl.toLowerCase());
}

function isPublicAuthRequest(request: HttpRequest<unknown>): boolean {
  const url = request.url.toLowerCase();
  return PUBLIC_AUTH_ENDPOINTS.some((endpoint) => url.includes(endpoint));
}

function withBearer(request: HttpRequest<unknown>, accessToken: string): HttpRequest<unknown> {
  return request.clone({
    setHeaders: {
      Authorization: `Bearer ${accessToken}`,
    },
  });
}

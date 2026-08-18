import { Injectable } from '@angular/core';
import {
  HttpInterceptor, HttpRequest,HttpHandler,HttpEvent, HttpErrorResponse} from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';

import { Router } from '@angular/router';

@Injectable()
export class AppInterceptor implements HttpInterceptor {

  constructor(
    private router: Router
  ) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {

    // ===== REQUEST =====
    const clonedRequest = req.clone({
      setHeaders: {

        // Authorization: '',
        // DeviceId: '',
        // UserId: '',

      }
    });

    return next.handle(clonedRequest).pipe(

      // ===== ERROR HANDLER =====
      catchError((error: HttpErrorResponse) => {

        switch (error.status) {

          case 401:
            // No autorizado
            // this.router.navigate(['/login']);
            break;

          case 403:
            // Sin permisos
            break;

          case 500:
            // Error servidor
            break;
        }

        return throwError(() => error);
      }),

      // ===== FINALIZE =====
      finalize(() => {

        // Ocultar loader
        // Finalizar procesos

      })

    );
  }
}
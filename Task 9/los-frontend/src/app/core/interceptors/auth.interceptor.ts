import { HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenService } from '../services/token.service';
import { Router } from '@angular/router';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const tokenService = inject(TokenService);
  const router = inject(Router);

  const credReq = req.clone({ withCredentials: true });

  return next(credReq).pipe(
    catchError((error) => {

      if (req.url.includes('/auth/login') ||
          req.url.includes('/auth/refresh') ||
          req.url.includes('/auth/logout')) {
        return throwError(() => error);
      }

      if (error.status === 401) {
        return authService.refresh().pipe(
          switchMap(() => {
            return next(req.clone({ withCredentials: true }));
          }),
          catchError((refreshError) => {
            tokenService.clear();
            void router.navigate(['/auth/login'],
              { queryParams: { reason: 'session-expired' } });
            return throwError(() => refreshError);
          })
        );
      }

      return throwError(() => error);
    })
  );
};
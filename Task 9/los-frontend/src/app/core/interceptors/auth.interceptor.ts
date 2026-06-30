import {
  HttpInterceptorFn, HttpRequest,
  HttpHandlerFn, HttpErrorResponse
} from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError, BehaviorSubject, filter, take } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenService } from '../services/token.service';
import { Router } from '@angular/router';

let isRefreshing = false;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const tokenService = inject(TokenService);
  const router = inject(Router);

  const credReq = addCredentials(req);

  return next(credReq).pipe(
    catchError((error: HttpErrorResponse) => {

      const isAuthEndpoint =
        req.url.includes('/auth/login')   ||
        req.url.includes('/auth/refresh') ||
        req.url.includes('/auth/logout')  ||
        req.url.includes('/auth/forgot-password') ||
        req.url.includes('/auth/reset-password');

      if (isAuthEndpoint || error.status !== 401) {
        return throwError(() => error);
      }
      if (!isRefreshing) {
        isRefreshing = true;

        return authService.refresh().pipe(
          switchMap(() => {
            isRefreshing = false;
            return next(addCredentials(req));
          }),
          catchError((refreshError) => {
            isRefreshing = false;
            tokenService.clear();
            void router.navigate(['/auth/login'],
              { queryParams: { reason: 'session-expired' } });
            return throwError(() => refreshError);
          })
        );
      }
      tokenService.clear();
      void router.navigate(['/auth/login'],
        { queryParams: { reason: 'session-expired' } });
      return throwError(() => error);
    })
  );
};

function addCredentials(req: HttpRequest<unknown>): HttpRequest<unknown> {
  return req.clone({ withCredentials: true });
}
import {
  HttpInterceptorFn, HttpRequest,
  HttpHandlerFn, HttpErrorResponse
} from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError, BehaviorSubject, filter, take } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenService } from '../services/token.service';
import { Router } from '@angular/router';

const AUTH_ENDPOINTS = [
  '/auth/login',
  '/auth/refresh',
  '/auth/logout',
  '/auth/forgot-password',
  '/auth/reset-password'
];
let isRefreshing = false;
const refreshDone$ = new BehaviorSubject<boolean | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const tokenService = inject(TokenService);
  const router = inject(Router);

  const credReq = addCredentials(req);

  return next(credReq).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthEndpoint = AUTH_ENDPOINTS.some((url) => req.url.includes(url));

      if (isAuthEndpoint || error.status !== 401) {
        return throwError(() => error);
      }

      if (isRefreshing) {
        return refreshDone$.pipe(
          filter((result) => result !== null),
          take(1),
          switchMap((succeeded) =>
            succeeded ? next(addCredentials(req)) : throwError(() => error)
          )
        );
      }

      isRefreshing = true;
      refreshDone$.next(null);

      return authService.refresh().pipe(
        switchMap(() => {
          isRefreshing = false;
          refreshDone$.next(true);
          return next(addCredentials(req));
        }),
        catchError((refreshError) => {
          isRefreshing = false;
          refreshDone$.next(false);
          tokenService.clear();
          void router.navigate(['/auth/login'],
            { queryParams: { reason: 'session-expired' } });
          return throwError(() => refreshError);
        })
      );
    })
  );
};

function addCredentials(req: HttpRequest<unknown>): HttpRequest<unknown> {
  return req.clone({ withCredentials: true });
}
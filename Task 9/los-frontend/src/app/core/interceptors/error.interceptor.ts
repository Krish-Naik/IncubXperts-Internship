import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { TokenService } from '../services/token.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const tokenService = inject(TokenService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 403) {
        void router.navigate(['/access-denied']);
      }

      if (error.status === 401 && !req.url.includes('/auth/')) {
        tokenService.clear();
        void router.navigate(['/auth/login'], { queryParams: { reason: 'session-expired' } });
      }

      const message =
        typeof error.error === 'object' && error.error?.message
          ? error.error.message
          : error.message;

      return throwError(() => new Error(message));
    })
  );
};

import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 403) {
        void router.navigate(['/access-denied']);
      }

      const message =
        typeof error.error === 'object' && error.error?.message
          ? error.error.message
          : error.message;

      return throwError(() => new Error(message));
    })
  );
};
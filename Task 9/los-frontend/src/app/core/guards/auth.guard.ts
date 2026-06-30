import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenService } from '../services/token.service';
import { UserContextService } from '../services/user-context.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const tokenService = inject(TokenService);
  const userContext = inject(UserContextService);
  const router = inject(Router);

  if (userContext.user()) {
    return true;
  }
  return authService.loadProfile().pipe(
    map(() => {
      return true;
    }),
    catchError(() => {
      tokenService.clear();
      return of(router.createUrlTree(['/auth/login']));
    })
  );
};
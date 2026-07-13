import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenService } from '../services/token.service';
import { UserContextService } from '../services/user-context.service';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const tokenService = inject(TokenService);
    const userContext = inject(UserContextService);
    const router = inject(Router);

    const checkRole = (): true | ReturnType<typeof router.createUrlTree> => {
      const role = userContext.role();
      if (role && allowedRoles.includes(role)) {
        return true;
      }
      return router.createUrlTree(['/access-denied']);
    };

    if (userContext.user()) {
      return checkRole();
    }

    return authService.loadProfile().pipe(
      map(() => checkRole()),
      catchError(() => {
        tokenService.clear();
        return of(router.createUrlTree(['/auth/login']));
      })
    );
  };
};
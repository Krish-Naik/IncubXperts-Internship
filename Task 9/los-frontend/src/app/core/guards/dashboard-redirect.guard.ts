import { Component, inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserContextService } from '../services/user-context.service';

@Component({
  selector: 'app-redirecting',
  standalone: true,
  template: `<p>Redirecting...</p>`
})
export class RedirectingComponent {}

// Mirrors AuthService.GetLandingRoute on the backend. authGuard runs before
// this on the same route tree, so userContext.user() is already populated.
const ROLE_LANDING_ROUTES: Record<string, string> = {
  SystemAdministrator: '/admin/users',
  VerificationOfficer: '/verification',
  BranchManager: '/approvals',
  Customer: '/my-application',
  BrokerAgent: '/broker/leads'
};

export const dashboardRedirectGuard: CanActivateFn = () => {
  const userContext = inject(UserContextService);
  const router = inject(Router);

  const role = userContext.role();
  const target = (role && ROLE_LANDING_ROUTES[role]) || '/admin/users';
  return router.createUrlTree([target]);
};
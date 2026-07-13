import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { dashboardRedirectGuard, RedirectingComponent } from './core/guards/dashboard-redirect.guard';
import { authRoutes } from './features/auth/auth.routes';
import { userManagementRoutes } from './features/user-management/user-management.routes';
import { AppLayoutComponent } from './layouts/app-layout.component';
import { AuthLayoutComponent } from './layouts/auth-layout.component';
import { AccessDeniedComponent } from './features/dashboard/dashboard-pages.component';
import { MyApplicationComponent } from './features/applications/pages/my-application.component';
import { ApprovalsDashboardComponent } from './features/approvals/pages/approvals-dashboard.component';
import { VerificationDashboardComponent } from './features/verification/pages/verification-dashboard.component';
import { BrokerDashboardComponent } from './features/broker/pages/broker-dashboard.component';

export const routes: Routes = [
  {
    path: 'auth',
    component: AuthLayoutComponent,
    children: authRoutes
  },
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'admin/users',
        canActivate: [roleGuard(['SystemAdministrator'])],
        children: userManagementRoutes
      },
      {
        path: 'verification',
        canActivate: [roleGuard(['VerificationOfficer', 'SystemAdministrator'])],
        component: VerificationDashboardComponent
      },
      {
        path: 'approvals',
        canActivate: [roleGuard(['BranchManager', 'SystemAdministrator'])],
        component: ApprovalsDashboardComponent
      },
      {
        path: 'my-application',
        canActivate: [roleGuard(['Customer', 'SystemAdministrator'])],
        component: MyApplicationComponent
      },
      {
        path: 'broker/leads',
        canActivate: [roleGuard(['BrokerAgent', 'SystemAdministrator'])],
        component: BrokerDashboardComponent
      },
      { path: 'access-denied', component: AccessDeniedComponent },
      {
        path: 'dashboard',
        canActivate: [dashboardRedirectGuard],
        component: RedirectingComponent
      }
    ]
  },
  { path: '**', redirectTo: 'auth/login' }
];
import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { authRoutes } from './features/auth/auth.routes';
import { userManagementRoutes } from './features/user-management/user-management.routes';
import { AppLayoutComponent } from './layouts/app-layout.component';
import { AuthLayoutComponent } from './layouts/auth-layout.component';
import {
  AccessDeniedComponent,
  ApprovalsDashboardComponent,
  BrokerDashboardComponent,
  CustomerDashboardComponent,
  VerificationDashboardComponent
} from './features/dashboard/dashboard-pages.component';

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
        component: CustomerDashboardComponent
      },
      {
        path: 'broker/leads',
        canActivate: [roleGuard(['BrokerAgent', 'SystemAdministrator'])],
        component: BrokerDashboardComponent
      },
      { path: 'access-denied', component: AccessDeniedComponent },
      { path: 'dashboard', redirectTo: 'admin/users', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'auth/login' }
];

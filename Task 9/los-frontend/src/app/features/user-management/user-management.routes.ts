import { Routes } from '@angular/router';
import { UsersListComponent } from './pages/users-list.component';
import { CreateUserComponent } from './pages/create-user.component';
import { EditUserComponent } from './pages/edit-user.component';
import { AssignRoleComponent } from './pages/assign-role.component';
import { AssignBranchComponent } from './pages/assign-branch.component';

export const userManagementRoutes: Routes = [
  { path: '', component: UsersListComponent },
  { path: 'create', component: CreateUserComponent },
  { path: ':id/edit', component: EditUserComponent },
  { path: ':id/assign-role', component: AssignRoleComponent },
  { path: ':id/assign-branch', component: AssignBranchComponent }
];

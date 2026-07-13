import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageHeaderComponent } from '../../../shared/components/page-header.component';
import { UserListItem } from '../../../core/models/user.model';
import { UserManagementService } from '../services/user-management.service';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [PageHeaderComponent, RouterLink],
  template: `
    <app-page-header title="User management" subtitle="Create and manage internal LOS users">
      <a class="btn" routerLink="/admin/users/create">Create user</a>
    </app-page-header>

    <table class="user-table">
      <thead>
        <tr>
          <th>Name</th>
          <th>Email</th>
          <th>Role</th>
          <th>Branch</th>
          <th>Status</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        @for (user of users(); track user.id) {
          <tr>
            <td>{{ user.fullName }}</td>
            <td>{{ user.email }}</td>
            <td>{{ user.role }}</td>
            <td>{{ user.branchName || '-' }}</td>
            <td>{{ user.status }}</td>
            <td class="actions">
              <a [routerLink]="['/admin/users', user.id, 'edit']">Edit</a>
              @if (user.status === 'Inactive' || user.status === 'Invited') {
                <button type="button" (click)="activate(user.id)">Activate</button>
              } @else {
                <button type="button" (click)="deactivate(user.id)">Deactivate</button>
              }
            </td>
          </tr>
        } @empty {
          <tr>
            <td colspan="6">No users found.</td>
          </tr>
        }
      </tbody>
    </table>
  `,
  styles: [
    `
      .btn {
        display: inline-block;
        padding: 0.6rem 1rem;
        background: #1f7a8c;
        color: #fff;
        text-decoration: none;
        border-radius: 8px;
      }
      .user-table {
        width: 100%;
        border-collapse: collapse;
        background: #fff;
        border-radius: 12px;
        overflow: hidden;
      }
      th,
      td {
        padding: 0.85rem 1rem;
        border-bottom: 1px solid #edf1f5;
        text-align: left;
      }
      th {
        background: #f7f9fc;
      }
      .actions a,
      .actions button {
        margin-right: 0.5rem;
      }
      button {
        border: 0;
        background: #edf2f7;
        padding: 0.35rem 0.65rem;
        border-radius: 6px;
        cursor: pointer;
      }
    `
  ]
})
export class UsersListComponent implements OnInit {
  private readonly userService = inject(UserManagementService);
  readonly users = signal<UserListItem[]>([]);

  ngOnInit(): void {
    this.loadUsers();
  }

  activate(id: string): void {
    this.userService.activateUser(id).subscribe(() => this.loadUsers());
  }

  deactivate(id: string): void {
    this.userService.deactivateUser(id).subscribe(() => this.loadUsers());
  }

  private loadUsers(): void {
    this.userService.getUsers().subscribe((users) => this.users.set(users));
  }
}

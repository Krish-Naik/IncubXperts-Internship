import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { PageHeaderComponent } from '../../../shared/components/page-header.component';
import { UserFormComponent } from '../components/user-form.component';
import { CreateUserRequest } from '../../../core/models/user.model';
import { UserManagementService } from '../services/user-management.service';

@Component({
  selector: 'app-create-user',
  standalone: true,
  imports: [PageHeaderComponent, UserFormComponent],
  template: `
    <app-page-header title="Create user" subtitle="New users receive an invitation to set their password" />
    <app-user-form mode="create" submitLabel="Create user" (save)="create($event)" />
    @if (message()) {
      <p class="success">{{ message() }}</p>
    }
    @if (error()) {
      <p class="error">{{ error() }}</p>
    }
  `,
  styles: [
    `
      .success {
        color: #2f855a;
      }
      .error {
        color: #c53030;
      }
    `
  ]
})
export class CreateUserComponent {
  private readonly userService = inject(UserManagementService);
  private readonly router = inject(Router);
  readonly message = signal('');
  readonly error = signal('');

  create(request: CreateUserRequest | unknown): void {
    this.userService.createUser(request as CreateUserRequest).subscribe({
      next: () => {
        this.message.set('User created. Check API logs for the invitation link in local development.');
        setTimeout(() => void this.router.navigate(['/admin/users']), 1000);
      },
      error: (err: Error) => this.error.set(err.message)
    });
  }
}

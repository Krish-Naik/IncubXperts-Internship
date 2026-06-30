import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PageHeaderComponent } from '../../../shared/components/page-header.component';
import { UserFormComponent } from '../components/user-form.component';
import { UpdateUserRequest, UserDetail } from '../../../core/models/user.model';
import { UserManagementService } from '../services/user-management.service';

@Component({
  selector: 'app-edit-user',
  standalone: true,
  imports: [PageHeaderComponent, UserFormComponent],
  template: `
    <app-page-header title="Edit user" [subtitle]="user()?.email ?? ''" />
    @if (user()) {
      <app-user-form
        mode="edit"
        submitLabel="Update user"
        [initialValue]="{
          employeeId: user()!.employeeId,
          fullName: user()!.fullName,
          email: user()!.email,
          phone: user()!.phone,
          roleId: user()!.roleId,
          branchId: user()!.branchId
        }"
        (save)="update($event)"
      />
    }
    @if (inviteStatus()) {
      <p>Invitation status: {{ inviteStatus()!.status }}</p>
      <button type="button" (click)="resendInvite()">Resend invite</button>
    }
    @if (error()) {
      <p class="error">{{ error() }}</p>
    }
  `,
  styles: [`.error { color: #c53030; }`]
})
export class EditUserComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly userService = inject(UserManagementService);
  readonly user = signal<UserDetail | null>(null);
  readonly inviteStatus = signal<{ status: string } | null>(null);
  readonly error = signal('');

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.userService.getUser(id).subscribe((user) => this.user.set(user));
    this.userService.getInviteStatus(id).subscribe((status) => this.inviteStatus.set(status));
  }

  update(request: UpdateUserRequest | unknown): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.userService.updateUser(id, request as UpdateUserRequest).subscribe({
      next: (user) => this.user.set(user),
      error: (err: Error) => this.error.set(err.message)
    });
  }

  resendInvite(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.userService.resendInvite(id).subscribe({
      next: () => this.error.set('Invitation resent. Check API logs for the link.'),
      error: (err: Error) => this.error.set(err.message)
    });
  }
}

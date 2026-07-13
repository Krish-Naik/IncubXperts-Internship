import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <form [formGroup]="form" (ngSubmit)="submit()">
      <h3>{{ isInvite() ? 'Set your password' : 'Choose a new password' }}</h3>
      <label>New password</label>
      <input formControlName="newPassword" type="password" />
      <label>Confirm password</label>
      <input formControlName="confirmPassword" type="password" />
      @if (message()) {
        <p class="success">{{ message() }}</p>
      }
      @if (error()) {
        <p class="error">{{ error() }}</p>
      }
      <button type="submit" [disabled]="form.invalid || loading()">Save password</button>
      <a routerLink="/auth/login">Back to login</a>
    </form>
  `,
  styles: [
    `
      form {
        display: grid;
        gap: 0.75rem;
      }
      .success {
        color: #2f855a;
      }
      .error {
        color: #c53030;
      }
      input,
      button {
        padding: 0.75rem;
        border-radius: 8px;
        border: 1px solid #cbd5e0;
      }
      button {
        background: #1f7a8c;
        color: #fff;
        border: 0;
      }
    `
  ]
})
export class ResetPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly message = signal('');
  readonly error = signal('');
  readonly isInvite = signal(this.route.snapshot.queryParamMap.get('invite') === 'true');
  private readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';

  readonly form = this.fb.nonNullable.group({
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required]
  });

  submit(): void {
    const { newPassword, confirmPassword } = this.form.getRawValue();
    if (newPassword !== confirmPassword) {
      this.error.set('Passwords do not match.');
      return;
    }
    if (!this.token) {
      this.error.set('Reset token is missing.');
      return;
    }

    this.loading.set(true);
    this.authService.resetPassword(this.token, newPassword).subscribe({
      next: (response) => {
        this.message.set(response.message);
        setTimeout(() => void this.router.navigate(['/auth/login']), 1200);
      },
      error: (err: Error) => this.error.set(err.message),
      complete: () => this.loading.set(false)
    });
  }
}

import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <form [formGroup]="form" (ngSubmit)="submit()">
      <h3>Reset your password</h3>
      <p>Enter your email and we will send reset instructions if the account exists.</p>
      <label>Email</label>
      <input formControlName="email" type="email" />
      @if (message()) {
        <p class="success">{{ message() }}</p>
      }
      @if (error()) {
        <p class="error">{{ error() }}</p>
      }
      <button type="submit" [disabled]="form.invalid || loading()">Send reset link</button>
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
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  readonly loading = signal(false);
  readonly message = signal('');
  readonly error = signal('');

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]]
  });

  submit(): void {
    this.loading.set(true);
    this.error.set('');
    this.authService.forgotPassword(this.form.controls.email.value).subscribe({
      next: (response) => this.message.set(response.message),
      error: (err: Error) => {
        this.error.set(err.message);
        this.loading.set(false);
      },
      complete: () => this.loading.set(false)
    });
  }
}

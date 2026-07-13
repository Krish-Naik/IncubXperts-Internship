import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <form [formGroup]="form" (ngSubmit)="submit()">
      @if (sessionMessage()) {
        <div class="banner">{{ sessionMessage() }}</div>
      }
      <label>Email</label>
      <input formControlName="email" type="email" />
      <label>Password</label>
      <input formControlName="password" type="password" />
      @if (error()) {
        <p class="error">{{ error() }}</p>
      }
      <button type="submit" [disabled]="form.invalid || loading()">Sign in</button>
      <a routerLink="/auth/forgot-password">Forgot password?</a>
    </form>
  `,
  styles: [
    `
      form {
        display: grid;
        gap: 0.75rem;
      }
      label {
        font-weight: 600;
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
      .error {
        color: #c53030;
      }
      .banner {
        background: #fff3cd;
        padding: 0.75rem;
        border-radius: 8px;
      }
      a {
        color: #1f7a8c;
      }
    `
  ]
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly loading = signal(false);
  readonly error = signal('');
  readonly sessionMessage = signal(
    this.route.snapshot.queryParamMap.get('reason') === 'session-expired'
      ? 'Your session expired. Please sign in again.'
      : ''
  );

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  submit(): void {
    if (this.form.invalid) {
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.authService.login(this.form.getRawValue()).subscribe({
      next: (response) => {
        void this.router.navigateByUrl(response.landingRoute);
      },
      error: (err: Error) => {
        this.error.set(err.message || 'Invalid email or password.');
        this.loading.set(false);
      },
      complete: () => this.loading.set(false)
    });
  }
}

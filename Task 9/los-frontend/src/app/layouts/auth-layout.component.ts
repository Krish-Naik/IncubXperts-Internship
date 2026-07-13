import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LogoComponent } from '../shared/components/logo.component';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet, LogoComponent],
  template: `
    <div class="auth-shell">
      <div class="auth-card">
        <div class="brand">
          <app-logo [size]="48" />
          <h2>Loan Origination System</h2>
          <p>Secure access for customers and internal teams</p>
        </div>
        <router-outlet />
      </div>
    </div>
  `,
  styles: [
    `
      .auth-shell {
        min-height: 100vh;
        display: grid;
        place-items: center;
        background: linear-gradient(135deg, #0f3d63, #1f7a8c);
        padding: 1.5rem;
      }
      .auth-card {
        width: min(100%, 460px);
        background: #fff;
        border-radius: 16px;
        padding: 2rem;
        box-shadow: 0 20px 50px rgba(0, 0, 0, 0.18);
      }
      .brand {
        display: flex;
        flex-direction: column;
        align-items: flex-start;
        gap: 0.75rem;
        margin-bottom: 0.5rem;
      }
      .brand h2 {
        margin: 0;
      }
      .brand p {
        color: #5b6472;
      }
    `
  ]
})
export class AuthLayoutComponent {}
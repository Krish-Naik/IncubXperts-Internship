import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/services/auth.service';
import { UserContextService } from '../core/services/user-context.service';

@Component({
  selector: 'app-app-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <div class="layout">
      <aside class="sidebar">
        <div class="logo">LOS</div>
        <nav>
          @if (userContext.role() === 'SystemAdministrator') {
            <a routerLink="/admin/users" routerLinkActive="active">Users</a>
          }
          @if (userContext.role() === 'VerificationOfficer') {
            <a routerLink="/verification" routerLinkActive="active">Verification Queue</a>
          }
          @if (userContext.role() === 'BranchManager') {
            <a routerLink="/approvals" routerLinkActive="active">Approvals</a>
          }
          @if (userContext.role() === 'Customer') {
            <a routerLink="/my-application" routerLinkActive="active">My Application</a>
          }
          @if (userContext.role() === 'BrokerAgent') {
            <a routerLink="/broker/leads" routerLinkActive="active">Leads</a>
          }
        </nav>
      </aside>
      <div class="main">
        <header class="topbar">
          <div>
            <strong>{{ userContext.user()?.fullName }}</strong>
            <span>{{ userContext.user()?.role }}</span>
          </div>
          <button type="button" (click)="logout()">Logout</button>
        </header>
        <main>
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  styles: [
    `
      .layout {
        display: grid;
        grid-template-columns: 240px 1fr;
        min-height: 100vh;
      }
      .sidebar {
        background: #102a43;
        color: #fff;
        padding: 1.5rem 1rem;
      }
      .logo {
        font-size: 1.4rem;
        font-weight: 700;
        margin-bottom: 2rem;
      }
      nav {
        display: grid;
        gap: 0.5rem;
      }
      a {
        color: #d9e2ec;
        text-decoration: none;
        padding: 0.75rem 1rem;
        border-radius: 8px;
      }
      a.active,
      a:hover {
        background: rgba(255, 255, 255, 0.12);
        color: #fff;
      }
      .topbar {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 1rem 1.5rem;
        border-bottom: 1px solid #dbe2ea;
        background: #fff;
      }
      .topbar span {
        display: block;
        color: #5b6472;
        font-size: 0.9rem;
      }
      main {
        padding: 1.5rem;
        background: #f4f7fb;
        min-height: calc(100vh - 72px);
      }
      button {
        border: 0;
        background: #1f7a8c;
        color: #fff;
        padding: 0.6rem 1rem;
        border-radius: 8px;
        cursor: pointer;
      }
    `
  ]
})
export class AppLayoutComponent {
  readonly userContext = inject(UserContextService);
  private readonly authService = inject(AuthService);

  logout(): void {
    this.authService.logout().subscribe();
  }
}

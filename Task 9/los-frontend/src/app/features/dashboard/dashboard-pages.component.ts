import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageHeaderComponent } from '../../shared/components/page-header.component';

@Component({
  selector: 'app-verification-dashboard',
  standalone: true,
  imports: [PageHeaderComponent],
  template: `
    <app-page-header
      title="Verification queue"
      subtitle="Review submitted documents and verify customer details"
    />
    <div class="card">Verification workflow placeholder for US-102 landing page.</div>
  `,
  styles: [`.card { background: #fff; padding: 1.5rem; border-radius: 12px; }`]
})
export class VerificationDashboardComponent {}

@Component({
  selector: 'app-approvals-dashboard',
  standalone: true,
  imports: [PageHeaderComponent],
  template: `
    <app-page-header
      title="Approval queue"
      subtitle="Review verified applications and approve loans"
    />
    <div class="card">Branch manager approval workflow placeholder.</div>
  `,
  styles: [`.card { background: #fff; padding: 1.5rem; border-radius: 12px; }`]
})
export class ApprovalsDashboardComponent {}

@Component({
  selector: 'app-broker-dashboard',
  standalone: true,
  imports: [PageHeaderComponent],
  template: `
    <app-page-header title="Broker leads" subtitle="Manage applicant leads and submissions" />
    <div class="card">Broker lead management placeholder.</div>
  `,
  styles: [`.card { background: #fff; padding: 1.5rem; border-radius: 12px; }`]
})
export class BrokerDashboardComponent {}

@Component({
  selector: 'app-access-denied',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="denied">
      <h1>Access denied</h1>
      <p>You do not have permission to view this page.</p>
      <a routerLink="/">Go back</a>
    </div>
  `,
  styles: [
    `
      .denied {
        min-height: 60vh;
        display: grid;
        place-items: center;
        text-align: center;
      }
    `
  ]
})
export class AccessDeniedComponent {}
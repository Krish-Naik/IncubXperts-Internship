import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PageHeaderComponent } from '../../../shared/components/page-header.component';
import { UserContextService } from '../../../core/services/user-context.service';
import { ApprovalsService } from '../services/approvals.service';
import { ApprovalQueueItem, DisbursementQueueItem } from '../../../core/models/approval.model';
import { ApplicationDetail } from '../../../core/models/application.model';

type DecisionForm = { interestRate: number | null; tenureMonths: number | null };
type RejectForm = { reason: string };
type InfoForm = { details: string };

@Component({
  selector: 'app-approvals-dashboard',
  standalone: true,
  imports: [PageHeaderComponent, FormsModule],
  template: `
    <app-page-header
      title="Approval queue"
      subtitle="Review verified applications and approve loans"
    />

    @if (error()) {
      <p class="error">{{ error() }}</p>
    }

    @if (loading()) {
      <p>Loading queue...</p>
    } @else if (queue().length === 0) {
      <div class="card">
        <p>No applications are waiting on a decision right now.</p>
      </div>
    }

    @for (item of queue(); track item.id) {
      <div class="card">
        <div class="row">
          <div>
            <strong>{{ item.referenceNumber }}</strong>
            <span class="muted">{{ item.customerName }} &middot; {{ item.loanType }} &middot; ₹{{ item.requestedAmount }} &middot; {{ item.requestedTenureMonths }} months</span>
          </div>
          <div class="row-actions">
            <button type="button" class="btn secondary" [disabled]="detailLoading() && detailItemId() === item.id" (click)="viewDetail(item)">
              {{ detailLoading() && detailItemId() === item.id ? 'Loading...' : 'View full details' }}
            </button>
            <span class="status" [class]="'status-' + item.status.toLowerCase()">{{ item.status }}</span>
          </div>
        </div>

        @if (item.status === 'Verified') {
          <div class="actions-block">
            <div class="inline-form">
              <label>
                Interest rate (%)
                <input type="number" step="0.01" min="0" [(ngModel)]="decisionForms[item.id].interestRate" [ngModelOptions]="{ standalone: true }" />
              </label>
              <label>
                Tenure (months)
                <input type="number" min="1" [(ngModel)]="decisionForms[item.id].tenureMonths" [ngModelOptions]="{ standalone: true }" />
              </label>
              <button
                type="button"
                [disabled]="busyId() === item.id"
                (click)="approve(item)"
              >
                {{ busyId() === item.id ? 'Working...' : 'Approve' }}
              </button>
            </div>
            <div class="inline-form">
              <label>
                Reason (for reject / info request)
                <input type="text" [(ngModel)]="rejectForms[item.id].reason" [ngModelOptions]="{ standalone: true }" placeholder="Why is this being rejected or held?" />
              </label>
              <button type="button" class="secondary" [disabled]="busyId() === item.id" (click)="reject(item)">
                Reject
              </button>
              <button type="button" class="secondary" [disabled]="busyId() === item.id" (click)="requestInfo(item)">
                Request info
              </button>
            </div>
          </div>
        }

        @if (item.status === 'PendingSeniorApproval') {
          @if (isAdmin()) {
            <div class="actions-block">
              <p class="muted">This loan exceeds the branch's high-value threshold and needs senior sign-off.</p>
              <div class="inline-form">
                <label>
                  Reason (if declining)
                  <input type="text" [(ngModel)]="rejectForms[item.id].reason" [ngModelOptions]="{ standalone: true }" />
                </label>
                <button type="button" [disabled]="busyId() === item.id" (click)="seniorDecision(item, true)">
                  {{ busyId() === item.id ? 'Working...' : 'Senior approve' }}
                </button>
                <button type="button" class="secondary" [disabled]="busyId() === item.id" (click)="seniorDecision(item, false)">
                  Senior decline
                </button>
              </div>
            </div>
          } @else {
            <p class="muted">Waiting on senior admin sign-off (high-value loan).</p>
          }
        }
      </div>
    }

    <app-page-header title="Ready to disburse" subtitle="Loans with an accepted offer" />

    @if (disbursementLoading()) {
      <p>Loading disbursement queue...</p>
    } @else if (disbursementQueue().length === 0) {
      <div class="card">
        <p>Nothing is waiting on disbursement.</p>
      </div>
    }

    @for (item of disbursementQueue(); track item.id) {
      <div class="card">
        <div class="row">
          <div>
            <strong>{{ item.referenceNumber }}</strong>
            <span class="muted">{{ item.customerName }} &middot; ₹{{ item.requestedAmount }} &middot; {{ item.approvedInterestRate }}% &middot; EMI ₹{{ item.monthlyEmi }}</span>
          </div>
        </div>
        <div class="actions-block">
          <div class="inline-form">
            <label>
              Bank reference
              <input type="text" [(ngModel)]="disburseForms[item.id]" [ngModelOptions]="{ standalone: true }" placeholder="UTR / transaction ref" />
            </label>
            <button type="button" [disabled]="busyId() === item.id" (click)="disburse(item)">
              {{ busyId() === item.id ? 'Disbursing...' : 'Disburse' }}
            </button>
          </div>
        </div>
      </div>
    }

    @if (detail()) {
      <div class="viewer-backdrop" (click)="closeDetail()">
        <div class="viewer-panel detail-panel" (click)="$event.stopPropagation()">
          <div class="viewer-header">
            <strong>{{ detail()!.referenceNumber }} &middot; {{ detail()!.status }}</strong>
            <button type="button" class="secondary" (click)="closeDetail()">Close</button>
          </div>
          <div class="detail-body">
            <section>
              <h4>Customer</h4>
              <p>{{ detail()!.customerName }} &middot; {{ detail()!.customerEmail }} &middot; {{ detail()!.customerPhone }}</p>
              @if (detail()!.brokerName) {
                <p class="muted">Referred by broker: {{ detail()!.brokerName }}</p>
              }
            </section>

            <section>
              <h4>Loan</h4>
              <p>
                {{ detail()!.loanType }} &middot; ₹{{ detail()!.requestedAmount }} requested &middot;
                {{ detail()!.requestedTenureMonths }} months
              </p>
              @if (detail()!.approvedInterestRate) {
                <p class="muted">
                  Approved: {{ detail()!.approvedInterestRate }}% for {{ detail()!.approvedTenureMonths }} months
                  &middot; EMI ₹{{ detail()!.monthlyEmi }}
                </p>
              }
            </section>

            @if (objectKeys(detail()!.extraDetails).length > 0) {
              <section>
                <h4>Loan-type details</h4>
                <ul class="kv-list">
                  @for (key of objectKeys(detail()!.extraDetails); track key) {
                    <li><span class="muted">{{ key }}</span> {{ detail()!.extraDetails[key] }}</li>
                  }
                </ul>
              </section>
            }

            @if (detail()!.coApplicants.length > 0) {
              <section>
                <h4>Co-applicants</h4>
                <ul class="kv-list">
                  @for (co of detail()!.coApplicants; track co.id) {
                    <li>{{ co.fullName }} &middot; {{ co.pan }} &middot; ₹{{ co.monthlyIncome }}/month</li>
                  }
                </ul>
              </section>
            }

            <section>
              <h4>Documents</h4>
              @if (detail()!.documents.length === 0) {
                <p class="muted">No documents uploaded yet.</p>
              }
              <ul class="kv-list">
                @for (doc of detail()!.documents; track doc.id) {
                  <li>
                    {{ doc.docType }} &middot; {{ doc.status }} &middot; {{ doc.originalFileName }}
                    @if (doc.uploadedByBroker) { &middot; uploaded by broker }
                    @if (doc.reviewerRemarks) { <br /><span class="muted">Remarks: {{ doc.reviewerRemarks }}</span> }
                  </li>
                }
              </ul>
            </section>

            @if (detail()!.infoRequestDetails) {
              <section>
                <h4>Info request history</h4>
                <p class="muted">Requested: {{ detail()!.infoRequestDetails }}</p>
                @if (detail()!.infoResponseText) {
                  <p class="muted">Customer response: {{ detail()!.infoResponseText }}</p>
                }
              </section>
            }

            @if (detail()!.rejectionReason) {
              <section>
                <h4>Rejection reason</h4>
                <p class="muted">{{ detail()!.rejectionReason }}</p>
              </section>
            }
          </div>
        </div>
      </div>
    }
  `,
  styles: [
    `
      .card {
        background: #fff;
        padding: 1.5rem;
        border-radius: 12px;
        margin-bottom: 1rem;
      }
      .row {
        display: flex;
        justify-content: space-between;
        align-items: center;
      }
      .row-actions {
        display: flex;
        gap: 0.75rem;
        align-items: center;
        flex-wrap: wrap;
      }
      .muted {
        display: block;
        color: #5b6472;
        font-size: 0.9rem;
        margin-top: 0.25rem;
      }
      .status {
        padding: 0.25rem 0.75rem;
        border-radius: 999px;
        background: #edf2f7;
        font-size: 0.85rem;
        white-space: nowrap;
      }
      .actions-block {
        margin-top: 1rem;
        border-top: 1px solid #edf1f5;
        padding-top: 1rem;
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
      }
      .inline-form {
        display: flex;
        gap: 0.75rem;
        align-items: flex-end;
        flex-wrap: wrap;
      }
      label {
        display: flex;
        flex-direction: column;
        font-size: 0.85rem;
        color: #5b6472;
        gap: 0.25rem;
      }
      input {
        padding: 0.6rem;
        border-radius: 8px;
        border: 1px solid #cbd5e0;
      }
      button,
      .btn {
        background: #1f7a8c;
        color: #fff;
        border: none;
        padding: 0.65rem 1rem;
        border-radius: 8px;
        cursor: pointer;
        height: fit-content;
      }
      button.secondary,
      .btn.secondary {
        background: #fff;
        color: #1f7a8c;
        border: 1px solid #1f7a8c;
      }
      button:disabled {
        opacity: 0.6;
        cursor: not-allowed;
      }
      .error {
        color: #c53030;
      }
      .viewer-backdrop {
        position: fixed;
        inset: 0;
        background: rgba(16, 42, 67, 0.65);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 1000;
        padding: 2rem;
      }
      .viewer-panel {
        background: #fff;
        border-radius: 12px;
        width: min(900px, 100%);
        height: min(85vh, 900px);
        display: flex;
        flex-direction: column;
        overflow: hidden;
      }
      .viewer-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 1rem 1.25rem;
        border-bottom: 1px solid #edf1f5;
      }
      .detail-panel {
        overflow-y: auto;
      }
      .detail-body {
        padding: 1.25rem;
        display: flex;
        flex-direction: column;
        gap: 1.25rem;
      }
      .detail-body h4 {
        margin: 0 0 0.5rem;
        font-size: 0.95rem;
      }
      .kv-list {
        list-style: none;
        padding: 0;
        margin: 0;
      }
      .kv-list li {
        padding: 0.35rem 0;
        border-bottom: 1px solid #edf1f5;
        font-size: 0.9rem;
      }
      .kv-list li .muted {
        margin-right: 0.5rem;
        font-weight: 600;
        display: inline;
      }
    `
  ]
})
export class ApprovalsDashboardComponent implements OnInit {
  private readonly approvalsService = inject(ApprovalsService);
  private readonly userContext = inject(UserContextService);

  readonly queue = signal<ApprovalQueueItem[]>([]);
  readonly disbursementQueue = signal<DisbursementQueueItem[]>([]);
  readonly loading = signal(true);
  readonly disbursementLoading = signal(true);
  readonly error = signal('');
  readonly busyId = signal<string | null>(null);

  readonly isAdmin = () => this.userContext.role() === 'SystemAdministrator';

  decisionForms: Record<string, DecisionForm> = {};
  rejectForms: Record<string, RejectForm> = {};
  infoForms: Record<string, InfoForm> = {};
  disburseForms: Record<string, string> = {};

  readonly detail = signal<ApplicationDetail | null>(null);
  readonly detailLoading = signal(false);
  readonly detailItemId = signal<string | null>(null);
  readonly objectKeys = Object.keys;

  ngOnInit(): void {
    this.loadQueue();
    this.loadDisbursementQueue();
  }

  private loadQueue(): void {
    this.loading.set(true);
    this.approvalsService.getQueue().subscribe({
      next: (items) => {
        this.queue.set(items);
        for (const item of items) {
          this.decisionForms[item.id] ??= { interestRate: null, tenureMonths: item.requestedTenureMonths };
          this.rejectForms[item.id] ??= { reason: '' };
          this.infoForms[item.id] ??= { details: '' };
        }
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.loading.set(false);
      }
    });
  }

  private loadDisbursementQueue(): void {
    this.disbursementLoading.set(true);
    this.approvalsService.getDisbursementQueue().subscribe({
      next: (items) => {
        this.disbursementQueue.set(items);
        this.disbursementLoading.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.disbursementLoading.set(false);
      }
    });
  }

  viewDetail(item: ApprovalQueueItem): void {
    this.error.set('');
    this.detailLoading.set(true);
    this.detailItemId.set(item.id);
    this.approvalsService.getApplicationDetail(item.id).subscribe({
      next: (d) => {
        this.detail.set(d);
        this.detailLoading.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.detailLoading.set(false);
      }
    });
  }

  closeDetail(): void {
    this.detail.set(null);
    this.detailItemId.set(null);
  }

  approve(item: ApprovalQueueItem): void {
    const form = this.decisionForms[item.id];
    if (!form.interestRate || !form.tenureMonths) {
      this.error.set('Enter an interest rate and tenure before approving.');
      return;
    }
    this.error.set('');
    this.busyId.set(item.id);
    this.approvalsService.approve(item.id, form.interestRate, form.tenureMonths).subscribe({
      next: () => {
        this.busyId.set(null);
        this.loadQueue();
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.busyId.set(null);
      }
    });
  }

  reject(item: ApprovalQueueItem): void {
    const reason = this.rejectForms[item.id].reason.trim();
    if (!reason) {
      this.error.set('A reason is required to reject an application.');
      return;
    }
    this.error.set('');
    this.busyId.set(item.id);
    this.approvalsService.reject(item.id, reason).subscribe({
      next: () => {
        this.busyId.set(null);
        this.loadQueue();
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.busyId.set(null);
      }
    });
  }

  requestInfo(item: ApprovalQueueItem): void {
    const details = this.rejectForms[item.id].reason.trim();
    if (!details) {
      this.error.set('Describe what additional information is needed.');
      return;
    }
    this.error.set('');
    this.busyId.set(item.id);
    this.approvalsService.requestInfo(item.id, details).subscribe({
      next: () => {
        this.busyId.set(null);
        this.loadQueue();
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.busyId.set(null);
      }
    });
  }

  seniorDecision(item: ApprovalQueueItem, approve: boolean): void {
    const reason = this.rejectForms[item.id]?.reason?.trim() || undefined;
    if (!approve && !reason) {
      this.error.set('A reason is required to decline at senior review.');
      return;
    }
    this.error.set('');
    this.busyId.set(item.id);
    this.approvalsService.seniorDecision(item.id, approve, reason).subscribe({
      next: () => {
        this.busyId.set(null);
        this.loadQueue();
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.busyId.set(null);
      }
    });
  }

  disburse(item: DisbursementQueueItem): void {
    const bankReference = (this.disburseForms[item.id] || '').trim();
    if (!bankReference) {
      this.error.set('Enter a bank reference before disbursing.');
      return;
    }
    this.error.set('');
    this.busyId.set(item.id);
    this.approvalsService.disburse(item.id, bankReference).subscribe({
      next: () => {
        this.busyId.set(null);
        this.loadDisbursementQueue();
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.busyId.set(null);
      }
    });
  }
}
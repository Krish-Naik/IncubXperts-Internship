import { Component, OnInit, inject, signal } from '@angular/core';
import {
  FormBuilder,
  FormsModule,
  ReactiveFormsModule,
  UntypedFormControl,
  UntypedFormGroup,
  Validators
} from '@angular/forms';
import { PageHeaderComponent } from '../../../shared/components/page-header.component';
import { KycUploadComponent } from './kyc-upload.component';
import { ApplicationService } from '../services/application.service';
import { DisbursementService, EmiScheduleRow } from '../services/disbursement.service';
import { LOAN_TYPE_FIELDS, LoanApplication, LoanType } from '../../../core/models/application.model';

@Component({
  selector: 'app-my-application',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, PageHeaderComponent, KycUploadComponent],
  template: `
    <app-page-header
      title="My application"
      subtitle="Track your loan application status or start a new application"
    >
      @if (!creating() && !hasOpenDraft()) {
        <button type="button" (click)="startCreate()">New application</button>
      }
    </app-page-header>

    @if (loading()) {
      <p>Loading your applications...</p>
    }

    @if (error()) {
      <p class="error">{{ error() }}</p>
    }

    @if (creating()) {
      <div class="card">
        <h2>Start a new loan application</h2>
        <form [formGroup]="form" (ngSubmit)="onCreate()" class="form-grid">
          <label>Loan type</label>
          <select formControlName="loanType" (change)="onLoanTypeChange()">
            @for (type of loanTypes; track type) {
              <option [value]="type">{{ type }}</option>
            }
          </select>

          <label>Requested amount (₹)</label>
          <input formControlName="requestedAmount" type="number" min="1" />

          <label>Requested tenure (months)</label>
          <input formControlName="requestedTenureMonths" type="number" min="1" />

          <div [formGroup]="extraFieldsForm" class="form-grid nested">
            @for (field of activeExtraFields(); track field.key) {
              <label>{{ field.label }}</label>
              <input [formControlName]="field.key" [placeholder]="field.label" />
            }
          </div>

          <div class="actions">
            <button type="submit" [disabled]="form.invalid || extraFieldsForm.invalid">Create draft</button>
            <button type="button" class="secondary" (click)="creating.set(false)">Cancel</button>
          </div>
        </form>
      </div>
    }

    @if (!loading() && applications().length === 0 && !creating()) {
      <div class="card">
        <p>You don't have any loan applications yet.</p>
        <button type="button" (click)="startCreate()">Start your first application</button>
      </div>
    }

    @for (app of applications(); track app.id) {
      <div class="card">
        <div class="app-summary">
          <div>
            <strong>{{ app.referenceNumber }}</strong>
            <span class="muted">{{ app.loanType }} &middot; ₹{{ app.requestedAmount }} &middot; {{ app.requestedTenureMonths }} months</span>
          </div>
          <span class="status" [class]="'status-' + app.status.toLowerCase()">{{ app.status }}</span>
        </div>

        @if (app.status === 'Draft' || app.status === 'InfoRequested') {
          <div class="draft-actions">
            <h4>Co-applicants</h4>
            @if (app.coApplicants && app.coApplicants.length > 0) {
              <ul class="co-applicant-list">
                @for (co of app.coApplicants; track co.id) {
                  <li>
                    {{ co.fullName }} &middot; {{ co.pan }} &middot; ₹{{ co.monthlyIncome }}/month
                    <button type="button" class="link-button" (click)="removeCoApplicant(app.id, co.id)">Remove</button>
                  </li>
                }
              </ul>
            } @else {
              <p class="muted">No co-applicants added yet.</p>
            }
            <div class="inline-form">
              <input placeholder="Full name" [(ngModel)]="coApplicantDrafts[app.id].fullName" [ngModelOptions]="{ standalone: true }" />
              <input placeholder="PAN" [(ngModel)]="coApplicantDrafts[app.id].pan" [ngModelOptions]="{ standalone: true }" />
              <input placeholder="Monthly income (₹)" type="number" [(ngModel)]="coApplicantDrafts[app.id].monthlyIncome" [ngModelOptions]="{ standalone: true }" />
              <button type="button" (click)="addCoApplicant(app.id)">Add co-applicant</button>
            </div>
          </div>
        }

        @if (app.status === 'Draft') {
          <div class="draft-actions">
            <p class="muted">Upload at least one KYC document, then submit your application for review.</p>
            <app-kyc-upload [applicationId]="app.id" />
            <button type="button" (click)="submit(app.id)" [disabled]="submitting() === app.id">
              {{ submitting() === app.id ? 'Submitting...' : 'Submit application' }}
            </button>
          </div>
        }

        @if (app.status === 'InfoRequested') {
          <div class="draft-actions info-requested">
            <p><strong>Your branch manager needs more information:</strong></p>
            <p class="muted">{{ app.infoRequestDetails }}</p>
            <app-kyc-upload [applicationId]="app.id" mode="single" />
            <label>Your response</label>
            <textarea [(ngModel)]="infoResponseDrafts[app.id]" [ngModelOptions]="{ standalone: true }" rows="3" placeholder="Explain, clarify, or confirm what was asked — a new document is only needed if requested."></textarea>
            <button type="button" (click)="respondToInfoRequest(app.id)" [disabled]="submitting() === app.id">
              {{ submitting() === app.id ? 'Resubmitting...' : 'Resubmit application' }}
            </button>
          </div>
        }

        @if (app.status === 'Rejected' && app.rejectionReason) {
          <div class="draft-actions">
            <p class="muted">Reason: {{ app.rejectionReason }}</p>
          </div>
        }

        @if (app.status === 'Approved') {
          <div class="draft-actions">
            <p class="muted">
              Offer: {{ app.approvedInterestRate }}% for {{ app.approvedTenureMonths }} months
              &middot; EMI ₹{{ app.monthlyEmi }}/month
            </p>
            <button type="button" (click)="acceptOffer(app.id)" [disabled]="submitting() === app.id">
              {{ submitting() === app.id ? 'Accepting...' : 'Accept offer' }}
            </button>
          </div>
        }

        @if (app.status === 'OfferAccepted') {
          <div class="draft-actions">
            <p class="muted">Offer accepted. Your branch manager will disburse the loan shortly.</p>
          </div>
        }

        @if (app.status === 'Disbursed') {
          <div class="draft-actions">
            <button type="button" (click)="toggleSchedule(app.id)">
              {{ scheduleAppId() === app.id ? 'Hide repayment schedule' : 'View repayment schedule' }}
            </button>
            @if (scheduleAppId() === app.id && schedule().length > 0) {
              <table class="schedule">
                <thead>
                  <tr><th>Month</th><th>EMI</th><th>Principal</th><th>Interest</th><th>Balance</th></tr>
                </thead>
                <tbody>
                  @for (row of schedule(); track row.month) {
                    <tr>
                      <td>{{ row.month }}</td>
                      <td>₹{{ row.emi }}</td>
                      <td>₹{{ row.principal }}</td>
                      <td>₹{{ row.interest }}</td>
                      <td>₹{{ row.balance }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            }
          </div>
        }
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
      .form-grid {
        display: grid;
        gap: 0.75rem;
        max-width: 420px;
        margin-top: 1rem;
      }
      .form-grid.nested {
        margin-top: 0;
      }
      input,
      select,
      textarea,
      button {
        padding: 0.75rem;
        border-radius: 8px;
        border: 1px solid #cbd5e0;
        font-family: inherit;
      }
      .actions {
        display: flex;
        gap: 0.75rem;
      }
      button {
        background: #1f7a8c;
        color: #fff;
        border: none;
        cursor: pointer;
      }
      button.secondary {
        background: #fff;
        color: #1f7a8c;
        border: 1px solid #1f7a8c;
      }
      button.link-button {
        background: none;
        border: none;
        color: #c53030;
        padding: 0;
        text-decoration: underline;
        cursor: pointer;
      }
      button:disabled {
        opacity: 0.6;
        cursor: not-allowed;
      }
      .app-summary {
        display: flex;
        justify-content: space-between;
        align-items: center;
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
      }
      .draft-actions {
        margin-top: 1rem;
        border-top: 1px solid #edf1f5;
        padding-top: 1rem;
      }
      .draft-actions label {
        display: block;
        margin-top: 0.5rem;
        font-size: 0.85rem;
        color: #5b6472;
      }
      .draft-actions textarea {
        width: 100%;
        margin: 0.5rem 0;
        resize: vertical;
      }
      .info-requested {
        background: #fffaf0;
        border-radius: 8px;
        padding: 1rem;
      }
      .co-applicant-list {
        list-style: none;
        padding: 0;
        margin: 0.5rem 0;
      }
      .co-applicant-list li {
        display: flex;
        justify-content: space-between;
        padding: 0.4rem 0;
        border-bottom: 1px solid #edf1f5;
        font-size: 0.9rem;
      }
      .inline-form {
        display: flex;
        gap: 0.5rem;
        flex-wrap: wrap;
        margin-top: 0.5rem;
      }
      .error {
        color: #c53030;
      }
      .schedule {
        width: 100%;
        border-collapse: collapse;
        margin-top: 0.75rem;
        font-size: 0.9rem;
      }
      .schedule th,
      .schedule td {
        text-align: left;
        padding: 0.4rem 0.6rem;
        border-bottom: 1px solid #edf1f5;
      }
    `
  ]
})
export class MyApplicationComponent implements OnInit {
  private readonly applicationService = inject(ApplicationService);
  private readonly disbursementService = inject(DisbursementService);
  private readonly fb = inject(FormBuilder);

  readonly applications = signal<LoanApplication[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly creating = signal(false);
  readonly submitting = signal<string | null>(null);
  readonly schedule = signal<EmiScheduleRow[]>([]);
  readonly scheduleAppId = signal<string | null>(null);
  readonly loanTypes = Object.values(LoanType);

  readonly infoResponseDrafts: Record<string, string> = {};
  readonly coApplicantDrafts: Record<string, { fullName: string; pan: string; monthlyIncome: number | null }> = {};

  readonly form = this.fb.group({
    loanType: [LoanType.Home, Validators.required],
    requestedAmount: [null as number | null, [Validators.required, Validators.min(1)]],
    requestedTenureMonths: [null as number | null, [Validators.required, Validators.min(1)]]
  });

  // Untyped on purpose: loan-type-specific keys are added/removed dynamically,
  // which a strictly-typed FormGroup (from fb.group) does not allow.
  readonly extraFieldsForm = new UntypedFormGroup({});

  ngOnInit(): void {
    this.load();
  }

  hasOpenDraft(): boolean {
    return this.applications().some((a) => a.status === 'Draft');
  }

  activeExtraFields() {
    const loanType = (this.form.get('loanType')?.value ?? LoanType.Home) as LoanType;
    return LOAN_TYPE_FIELDS[loanType];
  }

  startCreate(): void {
    this.creating.set(true);
    this.onLoanTypeChange();
  }

  onLoanTypeChange(): void {
    for (const key of Object.keys(this.extraFieldsForm.controls)) {
      this.extraFieldsForm.removeControl(key);
    }
    for (const field of this.activeExtraFields()) {
      this.extraFieldsForm.addControl(field.key, new UntypedFormControl('', Validators.required));
    }
  }

  load(): void {
    this.loading.set(true);
    this.applicationService.getMine().subscribe({
      next: (apps) => {
        this.applications.set(apps);
        for (const app of apps) {
          this.infoResponseDrafts[app.id] ??= '';
          this.coApplicantDrafts[app.id] ??= { fullName: '', pan: '', monthlyIncome: null };
        }
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.loading.set(false);
      }
    });
  }

  onCreate(): void {
    if (this.form.invalid || this.extraFieldsForm.invalid) return;
    const { loanType, requestedAmount, requestedTenureMonths } = this.form.value;
    const extraDetails = this.extraFieldsForm.value as Record<string, string>;

    this.applicationService
      .createDraft(loanType as LoanType, requestedAmount as number, requestedTenureMonths as number, extraDetails)
      .subscribe({
        next: (app) => {
          this.applications.update((list) => [app, ...list]);
          this.creating.set(false);
          this.form.reset({ loanType: LoanType.Home, requestedAmount: null, requestedTenureMonths: null });
          this.extraFieldsForm.reset();
        },
        error: (err: Error) => this.error.set(err.message)
      });
  }

  submit(applicationId: string): void {
    this.error.set('');
    this.submitting.set(applicationId);
    this.applicationService.submit(applicationId).subscribe({
      next: () => {
        this.submitting.set(null);
        this.load();
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.submitting.set(null);
      }
    });
  }

  respondToInfoRequest(applicationId: string): void {
    const responseText = (this.infoResponseDrafts[applicationId] || '').trim();
    if (!responseText) {
      this.error.set('Enter a response before resubmitting.');
      return;
    }
    this.error.set('');
    this.submitting.set(applicationId);
    this.applicationService.respondToInfoRequest(applicationId, responseText).subscribe({
      next: () => {
        this.submitting.set(null);
        this.infoResponseDrafts[applicationId] = '';
        this.load();
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.submitting.set(null);
      }
    });
  }

  addCoApplicant(applicationId: string): void {
    const draft = this.coApplicantDrafts[applicationId];
    if (!draft?.fullName?.trim() || !draft?.pan?.trim() || !draft?.monthlyIncome) {
      this.error.set("Fill in the co-applicant's name, PAN, and monthly income.");
      return;
    }
    this.error.set('');
    this.applicationService
      .addCoApplicant(applicationId, draft.fullName.trim(), draft.pan.trim(), draft.monthlyIncome)
      .subscribe({
        next: () => {
          this.coApplicantDrafts[applicationId] = { fullName: '', pan: '', monthlyIncome: null };
          this.load();
        },
        error: (err: Error) => this.error.set(err.message)
      });
  }

  removeCoApplicant(applicationId: string, coApplicantId: string): void {
    this.error.set('');
    this.applicationService.removeCoApplicant(applicationId, coApplicantId).subscribe({
      next: () => this.load(),
      error: (err: Error) => this.error.set(err.message)
    });
  }

  acceptOffer(applicationId: string): void {
    this.error.set('');
    this.submitting.set(applicationId);
    this.disbursementService.acceptOffer(applicationId).subscribe({
      next: () => {
        this.submitting.set(null);
        this.load();
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.submitting.set(null);
      }
    });
  }

  // Fixed: this used to always call getSchedule() and re-fetch, so the button could
  // never actually collapse the table. Now it toggles scheduleAppId instead.
  toggleSchedule(applicationId: string): void {
    if (this.scheduleAppId() === applicationId) {
      this.scheduleAppId.set(null);
      this.schedule.set([]);
      return;
    }
    this.error.set('');
    this.disbursementService.getSchedule(applicationId).subscribe({
      next: (rows) => {
        this.schedule.set(rows);
        this.scheduleAppId.set(applicationId);
      },
      error: (err: Error) => this.error.set(err.message)
    });
  }
}
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PageHeaderComponent } from '../../../shared/components/page-header.component';
import { BrokerService } from '../services/broker.service';
import { ReferredApplicationRow } from '../../../core/models/broker.model';

@Component({
  selector: 'app-broker-dashboard',
  standalone: true,
  imports: [PageHeaderComponent, ReactiveFormsModule],
  template: `
    <app-page-header title="Broker leads" subtitle="Manage applicant leads and submissions" />

    <div class="card">
      <h2>Register a new lead</h2>
      <p class="muted">
        The lead is created as a customer account and receives an invitation to set a password
        and complete their own loan application.
      </p>
      <form [formGroup]="leadForm" (ngSubmit)="registerLead()" class="form-grid">
        <label>Full name</label>
        <input formControlName="fullName" />
        <label>Email</label>
        <input formControlName="email" type="email" />
        <label>Phone</label>
        <input formControlName="phone" />
        <button type="submit" [disabled]="leadForm.invalid || registeringLead()">
          {{ registeringLead() ? 'Registering...' : 'Register lead' }}
        </button>
      </form>
      @if (leadMessage()) {
        <p class="success">{{ leadMessage() }}</p>
      }
      @if (leadError()) {
        <p class="error">{{ leadError() }}</p>
      }
    </div>

    <div class="card">
      <h2>Upload documents on a customer's behalf</h2>
      <p class="muted">
        Pick one of your referred applications below, then upload their Aadhaar or PAN.
      </p>
      <form [formGroup]="uploadForm" (ngSubmit)="uploadDocument()" class="form-grid">
        <label>Application</label>
        <select formControlName="applicationId">
          <option value="" disabled>Select an application</option>
          @for (row of pipeline(); track row.id) {
            <option [value]="row.id">{{ row.referenceNumber }} &middot; {{ row.status }}</option>
          }
        </select>
        <label>Document type</label>
        <select formControlName="docType">
          <option value="Aadhaar">Aadhaar</option>
          <option value="Pan">PAN</option>
        </select>
        <label>File</label>
        <input type="file" accept=".pdf,.jpg,.jpeg,.png" (change)="onFileSelected($event)" />
        <button type="submit" [disabled]="uploadForm.invalid || !selectedFile() || uploading()">
          {{ uploading() ? 'Uploading...' : 'Upload document' }}
        </button>
      </form>
      @if (uploadMessage()) { <p class="success">{{ uploadMessage() }}</p> }
      @if (uploadError()) { <p class="error">{{ uploadError() }}</p> }
    </div>

    <div class="card">
      <h2>Your pipeline</h2>
      @if (pipelineLoading()) {
        <p>Loading pipeline...</p>
      } @else if (pipeline().length === 0) {
        <p>No referred applications yet. They'll show up here as soon as a lead you registered creates a loan application.</p>
      } @else {
        <table class="pipeline-table">
          <thead>
            <tr>
              <th>Reference</th>
              <th>Status</th>
              <th>Commission</th>
            </tr>
          </thead>
          <tbody>
            @for (row of pipeline(); track row.referenceNumber) {
              <tr>
                <td>{{ row.referenceNumber }}</td>
                <td>{{ row.status }}</td>
                <td>{{ row.commission ? '₹' + row.commission : '-' }}</td>
              </tr>
            }
          </tbody>
        </table>
      }
      @if (pipelineError()) {
        <p class="error">{{ pipelineError() }}</p>
      }
    </div>
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
      input,
      select,
      button {
        padding: 0.75rem;
        border-radius: 8px;
        border: 1px solid #cbd5e0;
      }
      button {
        background: #1f7a8c;
        color: #fff;
        border: none;
        cursor: pointer;
        justify-self: start;
      }
      button:disabled {
        opacity: 0.6;
        cursor: not-allowed;
      }
      .muted {
        color: #5b6472;
        font-size: 0.9rem;
      }
      .success {
        color: #2f855a;
      }
      .error {
        color: #c53030;
      }
      .pipeline-table {
        width: 100%;
        border-collapse: collapse;
      }
      .pipeline-table th,
      .pipeline-table td {
        text-align: left;
        padding: 0.6rem;
        border-bottom: 1px solid #edf1f5;
      }
    `
  ]
})
export class BrokerDashboardComponent implements OnInit {
  private readonly brokerService = inject(BrokerService);
  private readonly fb = inject(FormBuilder);

  readonly registeringLead = signal(false);
  readonly leadMessage = signal('');
  readonly leadError = signal('');

  readonly uploading = signal(false);
  readonly uploadMessage = signal('');
  readonly uploadError = signal('');
  readonly selectedFile = signal<File | null>(null);

  readonly pipeline = signal<ReferredApplicationRow[]>([]);
  readonly pipelineLoading = signal(true);
  readonly pipelineError = signal('');

  readonly leadForm = this.fb.nonNullable.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', Validators.required]
  });

  readonly uploadForm = this.fb.nonNullable.group({
    applicationId: ['', Validators.required],
    docType: ['Aadhaar' as 'Aadhaar' | 'Pan', Validators.required]
  });

  ngOnInit(): void {
    this.loadPipeline();
  }

  registerLead(): void {
    if (this.leadForm.invalid || this.registeringLead()) return;
    this.leadError.set('');
    this.leadMessage.set('');
    this.registeringLead.set(true);
    this.brokerService.registerLead(this.leadForm.getRawValue()).subscribe({
      next: (lead) => {
        this.registeringLead.set(false);
        this.leadMessage.set(`Lead registered: ${lead.fullName} (${lead.email}). They'll receive an invite to log in.`);
        this.leadForm.reset();
      },
      error: (err: Error) => {
        this.registeringLead.set(false);
        this.leadError.set(err.message);
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0] ?? null);
  }

  uploadDocument(): void {
    const file = this.selectedFile();
    if (this.uploadForm.invalid || !file || this.uploading()) return;
    const { applicationId, docType } = this.uploadForm.getRawValue();
    this.uploadError.set('');
    this.uploadMessage.set('');
    this.uploading.set(true);
    this.brokerService.uploadOnBehalf(applicationId, docType, file).subscribe({
      next: () => {
        this.uploading.set(false);
        this.uploadMessage.set('Document uploaded.');
        this.selectedFile.set(null);
        this.loadPipeline();
      },
      error: (err: Error) => {
        this.uploading.set(false);
        this.uploadError.set(err.message);
      }
    });
  }

  private loadPipeline(): void {
    this.pipelineLoading.set(true);
    this.brokerService.getPipeline().subscribe({
      next: (rows) => {
        this.pipeline.set(rows);
        this.pipelineLoading.set(false);
      },
      error: (err: Error) => {
        this.pipelineError.set(err.message);
        this.pipelineLoading.set(false);
      }
    });
  }
}

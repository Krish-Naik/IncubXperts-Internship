import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { PageHeaderComponent } from '../../../shared/components/page-header.component';
import { KycService } from '../../kyc/services/kyc.service';
import { KycQueueItem } from '../../../core/models/kyc.model';

@Component({
  selector: 'app-verification-dashboard',
  standalone: true,
  imports: [PageHeaderComponent, FormsModule],
  template: `
    <app-page-header
      title="Verification queue"
      subtitle="Review submitted documents and verify customer details"
    />

    @if (error()) {
      <p class="error">{{ error() }}</p>
    }

    @if (loading()) {
      <p>Loading queue...</p>
    } @else if (queue().length === 0) {
      <div class="card">
        <p>Nothing pending review. Nice and clear.</p>
      </div>
    }

    @for (item of queue(); track item.id) {
      <div class="card">
        <div class="row">
          <div>
            <strong>{{ item.referenceNumber }}</strong>
            <span class="muted">
              {{ item.customerName }} &middot; {{ item.docType }} &middot; {{ item.originalFileName }}
              @if (item.uploadedByBroker) { &middot; uploaded by broker }
            </span>
          </div>
          <button type="button" class="btn secondary" [disabled]="viewerLoading()" (click)="viewDocument(item)">
            {{ viewerLoading() && viewingId() === item.id ? 'Loading...' : 'View document' }}
          </button>
        </div>

        <div class="actions-block">
          <button type="button" [disabled]="busyId() === item.id" (click)="approve(item)">
            {{ busyId() === item.id ? 'Working...' : 'Approve' }}
          </button>
          <div class="inline-form">
            <label>
              Remarks (required to reject)
              <input type="text" [(ngModel)]="remarksForms[item.id]" [ngModelOptions]="{ standalone: true }" placeholder="What's wrong with this document?" />
            </label>
            <button type="button" class="secondary" [disabled]="busyId() === item.id" (click)="reject(item)">
              Reject
            </button>
          </div>
        </div>
      </div>
    }

    @if (viewerUrl()) {
      <div class="viewer-backdrop" (click)="closeViewer()">
        <div class="viewer-panel" (click)="$event.stopPropagation()">
          <div class="viewer-header">
            <strong>{{ viewingFileName() }}</strong>
            <button type="button" class="secondary" (click)="closeViewer()">Close</button>
          </div>
          @if (viewingIsImage()) {
            <img [src]="viewerUrl()" [alt]="viewingFileName()" class="viewer-image" />
          } @else {
            <iframe [src]="viewerUrl()" class="viewer-frame" title="Document preview"></iframe>
          }
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
        gap: 1rem;
      }
      .muted {
        display: block;
        color: #5b6472;
        font-size: 0.9rem;
        margin-top: 0.25rem;
      }
      .actions-block {
        margin-top: 1rem;
        border-top: 1px solid #edf1f5;
        padding-top: 1rem;
        display: flex;
        gap: 0.75rem;
        align-items: flex-end;
        flex-wrap: wrap;
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
        min-width: 240px;
      }
      button,
      .btn {
        background: #1f7a8c;
        color: #fff;
        border: none;
        padding: 0.65rem 1rem;
        border-radius: 8px;
        cursor: pointer;
        text-decoration: none;
        display: inline-block;
        white-space: nowrap;
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
      .viewer-frame {
        flex: 1;
        border: 0;
        width: 100%;
      }
      .viewer-image {
        flex: 1;
        width: 100%;
        object-fit: contain;
        background: #0b1f33;
      }
    `
  ]
})
export class VerificationDashboardComponent implements OnInit, OnDestroy {
  private readonly kycService = inject(KycService);
  private readonly sanitizer = inject(DomSanitizer);

  readonly queue = signal<KycQueueItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly busyId = signal<string | null>(null);
  remarksForms: Record<string, string> = {};

  readonly viewerUrl = signal<SafeResourceUrl | null>(null);
  readonly viewerLoading = signal(false);
  readonly viewingId = signal<string | null>(null);
  readonly viewingFileName = signal('');
  readonly viewingIsImage = signal(false);
  private viewerObjectUrl: string | null = null;

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.revokeViewerObjectUrl();
  }

  fileUrl(documentId: string): string {
    return this.kycService.getFileUrl(documentId);
  }

  viewDocument(item: KycQueueItem): void {
    this.error.set('');
    this.viewerLoading.set(true);
    this.viewingId.set(item.id);
    this.kycService.getFileBlob(item.id).subscribe({
      next: (blob) => {
        this.revokeViewerObjectUrl();
        this.viewerObjectUrl = URL.createObjectURL(blob);
        this.viewingIsImage.set(blob.type.startsWith('image/'));
        this.viewingFileName.set(item.originalFileName);
        this.viewerUrl.set(this.sanitizer.bypassSecurityTrustResourceUrl(this.viewerObjectUrl));
        this.viewerLoading.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message || 'Could not load the document.');
        this.viewerLoading.set(false);
        this.viewingId.set(null);
      }
    });
  }

  closeViewer(): void {
    this.viewerUrl.set(null);
    this.viewingId.set(null);
    this.revokeViewerObjectUrl();
  }

  private revokeViewerObjectUrl(): void {
    if (this.viewerObjectUrl) {
      URL.revokeObjectURL(this.viewerObjectUrl);
      this.viewerObjectUrl = null;
    }
  }

  private load(): void {
    this.loading.set(true);
    this.kycService.getReviewQueue().subscribe({
      next: (items) => {
        this.queue.set(items);
        for (const item of items) {
          this.remarksForms[item.id] ??= '';
        }
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.loading.set(false);
      }
    });
  }

  approve(item: KycQueueItem): void {
    this.error.set('');
    this.busyId.set(item.id);
    this.kycService.reviewDocument(item.id, 'Approved').subscribe({
      next: () => {
        this.busyId.set(null);
        if (this.viewingId() === item.id) this.closeViewer();
        this.load();
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.busyId.set(null);
      }
    });
  }

  reject(item: KycQueueItem): void {
    const remarks = (this.remarksForms[item.id] || '').trim();
    if (!remarks) {
      this.error.set('Remarks are mandatory when rejecting a document.');
      return;
    }
    this.error.set('');
    this.busyId.set(item.id);
    this.kycService.reviewDocument(item.id, 'Rejected', remarks).subscribe({
      next: () => {
        this.busyId.set(null);
        if (this.viewingId() === item.id) this.closeViewer();
        this.load();
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.busyId.set(null);
      }
    });
  }
}
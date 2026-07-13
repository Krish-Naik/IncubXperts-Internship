import { Component, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { KycService } from '../../kyc/services/kyc.service';

@Component({
  selector: 'app-kyc-upload',
  standalone: true,
  imports: [FormsModule],
  template: `
    @if (mode() === 'dual') {
      <div class="upload-box">
        <label>Aadhaar
          <input type="file" accept=".pdf,.jpg,.jpeg,.png" (change)="onFileSelected($event, 'Aadhaar')" />
        </label>
        <label>PAN
          <input type="file" accept=".pdf,.jpg,.jpeg,.png" (change)="onFileSelected($event, 'Pan')" />
        </label>
        @if (errorMsg()) { <p class="error">{{ errorMsg() }}</p> }
        @if (successMsg()) { <p class="success">{{ successMsg() }}</p> }
      </div>
    } @else {
      <div class="upload-box single">
        <p class="muted">Optional — only upload a new document if the manager specifically asked for one.</p>
        <div class="single-row">
          <select [(ngModel)]="selectedDocType" [ngModelOptions]="{ standalone: true }">
            <option value="Aadhaar">Aadhaar</option>
            <option value="Pan">PAN</option>
          </select>
          <input type="file" accept=".pdf,.jpg,.jpeg,.png" (change)="onFileSelected($event, selectedDocType)" />
        </div>
        @if (errorMsg()) { <p class="error">{{ errorMsg() }}</p> }
        @if (successMsg()) { <p class="success">{{ successMsg() }}</p> }
      </div>
    }
  `,
  styles: [
    `
      .error { color: #c53030; }
      .success { color: #2f855a; }
      .muted { color: #5b6472; font-size: 0.85rem; margin-bottom: 0.5rem; }
      .single-row { display: flex; gap: 0.5rem; align-items: center; flex-wrap: wrap; }
      label {
        display: block;
        margin-bottom: 0.5rem;
      }
    `
  ]
})
export class KycUploadComponent {
  private kycService = inject(KycService);
  applicationId = input.required<string>();

  // 'dual' (Draft): both Aadhaar + PAN inputs, as before.
  // 'single' (InfoRequested response): one optional upload, doc type picked from a dropdown.
  mode = input<'dual' | 'single'>('dual');

  selectedDocType: 'Aadhaar' | 'Pan' = 'Aadhaar';

  readonly errorMsg = signal('');
  readonly successMsg = signal('');

  onFileSelected(event: Event, docType: 'Aadhaar' | 'Pan'): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const allowed = ['application/pdf', 'image/jpeg', 'image/png'];
    if (!allowed.includes(file.type)) {
      this.errorMsg.set('Unsupported file type. Allowed: PDF, JPG, PNG.');
      this.successMsg.set('');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.errorMsg.set('File exceeds 5 MB limit.');
      this.successMsg.set('');
      return;
    }
    this.errorMsg.set('');
    this.successMsg.set('');
    this.kycService.uploadDocument(this.applicationId(), docType, file).subscribe({
      next: () => this.successMsg.set(`${docType} uploaded successfully.`),
      error: (err: Error) => this.errorMsg.set(err.message)
    });
  }
}
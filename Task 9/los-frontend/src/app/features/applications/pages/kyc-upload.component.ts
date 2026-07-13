import { Component, inject, input, signal } from '@angular/core';
import { KycService } from '../../kyc/services/kyc.service';

@Component({
  selector: 'app-kyc-upload',
  standalone: true,
  template: `
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
  `,
  styles: [
    `
      .error { color: #c53030; }
      .success { color: #2f855a; }
    `
  ]
})
export class KycUploadComponent {
  private kycService = inject(KycService);
  applicationId = input.required<string>();

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
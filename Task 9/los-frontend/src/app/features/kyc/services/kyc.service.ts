import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { KycQueueItem } from '../../../core/models/kyc.model';
import { ApplicationDetail } from '../../../core/models/application.model';

@Injectable({ providedIn: 'root' })
export class KycService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/kyc`;

  uploadDocument(
    applicationId: string,
    docType: 'Aadhaar' | 'Pan',
    file: File
  ): Observable<unknown> {
    const formData = new FormData();
    formData.append('docType', docType);
    formData.append('file', file);
    return this.http.post(`${this.base}/${applicationId}/upload`, formData);
  }

  getReviewQueue(): Observable<KycQueueItem[]> {
    return this.http.get<KycQueueItem[]>(`${this.base}/queue`);
  }

  getApplicationDetail(applicationId: string): Observable<ApplicationDetail> {
    return this.http.get<ApplicationDetail>(`${this.base}/applications/${applicationId}/detail`);
  }

  reviewDocument(
    documentId: string,
    status: 'Approved' | 'Rejected',
    remarks?: string
  ): Observable<unknown> {
    return this.http.post(`${this.base}/${documentId}/review`, { status, remarks });
  }

  getFileUrl(documentId: string): string {
    return `${this.base}/${documentId}/file`;
  }

  getFileBlob(documentId: string): Observable<Blob> {
    return this.http.get(`${this.base}/${documentId}/file`, { responseType: 'blob' });
  }
}
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { LeadResult, ReferredApplicationRow, RegisterLeadRequest } from '../../../core/models/broker.model';

@Injectable({ providedIn: 'root' })
export class BrokerService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/broker`;

  registerLead(request: RegisterLeadRequest): Observable<LeadResult> {
    return this.http.post<LeadResult>(`${this.base}/leads`, request);
  }

  getPipeline(from?: string, to?: string): Observable<ReferredApplicationRow[]> {
    const params: Record<string, string> = {};
    if (from) params['from'] = from;
    if (to) params['to'] = to;
    return this.http.get<ReferredApplicationRow[]>(`${this.base}/pipeline`, { params });
  }

  uploadOnBehalf(applicationId: string, docType: 'Aadhaar' | 'Pan', file: File): Observable<unknown> {
    const formData = new FormData();
    formData.append('docType', docType);
    formData.append('file', file);
    return this.http.post(`${this.base}/${applicationId}/upload-document`, formData);
  }
}

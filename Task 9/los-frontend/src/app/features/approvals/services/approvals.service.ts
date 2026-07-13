import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { LoanApplication } from '../../../core/models/application.model';
import { ApprovalQueueItem, DisbursementQueueItem } from '../../../core/models/approval.model';

@Injectable({ providedIn: 'root' })
export class ApprovalsService {
  private http = inject(HttpClient);
  private approvalsBase = `${environment.apiUrl}/approvals`;
  private disbursementBase = `${environment.apiUrl}/disbursement`;

  getQueue(): Observable<ApprovalQueueItem[]> {
    return this.http.get<ApprovalQueueItem[]>(`${this.approvalsBase}/queue`);
  }

  approve(id: string, interestRate: number, tenureMonths: number): Observable<LoanApplication> {
    return this.http.post<LoanApplication>(`${this.approvalsBase}/${id}/approve`, {
      interestRate,
      tenureMonths
    });
  }

  reject(id: string, reason: string): Observable<LoanApplication> {
    return this.http.post<LoanApplication>(`${this.approvalsBase}/${id}/reject`, { reason });
  }

  requestInfo(id: string, details: string): Observable<LoanApplication> {
    return this.http.post<LoanApplication>(`${this.approvalsBase}/${id}/request-info`, { details });
  }

  seniorDecision(id: string, approve: boolean, reason?: string): Observable<LoanApplication> {
    return this.http.post<LoanApplication>(`${this.approvalsBase}/${id}/senior-decision`, {
      approve,
      reason
    });
  }

  getDisbursementQueue(): Observable<DisbursementQueueItem[]> {
    return this.http.get<DisbursementQueueItem[]>(`${this.disbursementBase}/queue`);
  }

  disburse(id: string, bankReference: string): Observable<LoanApplication> {
    return this.http.post<LoanApplication>(`${this.disbursementBase}/${id}/disburse`, {
      bankReference
    });
  }
}

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CoApplicant, LoanApplication, LoanType } from '../../../core/models/application.model';

@Injectable({ providedIn: 'root' })
export class ApplicationService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/applications`;

  createDraft(
    loanType: LoanType,
    requestedAmount: number,
    requestedTenureMonths: number,
    extraDetails?: Record<string, string>
  ): Observable<LoanApplication> {
    return this.http.post<LoanApplication>(`${this.base}/draft`, {
      loanType,
      requestedAmount,
      requestedTenureMonths,
      extraDetails: extraDetails ?? null
    });
  }

  submit(id: string): Observable<LoanApplication> {
    return this.http.post<LoanApplication>(`${this.base}/${id}/submit`, {});
  }

  respondToInfoRequest(id: string, responseText: string): Observable<LoanApplication> {
    return this.http.post<LoanApplication>(`${this.base}/${id}/respond-info`, { responseText });
  }

  addCoApplicant(id: string, fullName: string, pan: string, monthlyIncome: number): Observable<CoApplicant> {
    return this.http.post<CoApplicant>(`${this.base}/${id}/co-applicants`, { fullName, pan, monthlyIncome });
  }

  removeCoApplicant(id: string, coApplicantId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}/co-applicants/${coApplicantId}`);
  }

  getMine(): Observable<LoanApplication[]> {
    return this.http.get<LoanApplication[]>(`${this.base}/mine`);
  }
}
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { LoanApplication } from '../../../core/models/application.model';

export interface EmiScheduleRow {
  month: number;
  emi: number;
  principal: number;
  interest: number;
  balance: number;
}

@Injectable({ providedIn: 'root' })
export class DisbursementService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/disbursement`;

  acceptOffer(applicationId: string): Observable<LoanApplication> {
    return this.http.post<LoanApplication>(`${this.base}/${applicationId}/accept-offer`, {});
  }

  getSchedule(applicationId: string): Observable<EmiScheduleRow[]> {
    return this.http.get<EmiScheduleRow[]>(`${this.base}/${applicationId}/schedule`);
  }
}
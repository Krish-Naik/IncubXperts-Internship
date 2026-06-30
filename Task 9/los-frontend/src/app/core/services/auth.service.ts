import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { EMPTY, catchError, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LoginRequest,
  LoginResponse,
  MessageResponse,
  UserProfile
} from '../models/auth.model';
import { TokenService } from './token.service';
import { UserContextService } from './user-context.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  constructor(
    private readonly http: HttpClient,
    private readonly tokenService: TokenService,
    private readonly userContext: UserContextService,
    private readonly router: Router
  ) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.baseUrl}/login`, request, {
        withCredentials: true
      })
      .pipe(tap((response) => this.applySession(response)));
  }

  refresh(): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.baseUrl}/refresh`, {}, {
        withCredentials: true
      })
      .pipe(tap((response) => this.applySession(response)));
  }

  logout(): Observable<void> {
    return this.http
      .post<void>(`${this.baseUrl}/logout`, {}, { withCredentials: true })
      .pipe(
        tap(() => {
          this.tokenService.clear();
          this.userContext.setUser(null);
          void this.router.navigate(['/auth/login']);
        }),
        catchError(() => {
          this.tokenService.clear();
          this.userContext.setUser(null);
          void this.router.navigate(['/auth/login']);
          return EMPTY;
        })
      );
  }

  forgotPassword(email: string): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.baseUrl}/forgot-password`, { email });
  }

  resetPassword(token: string, newPassword: string): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.baseUrl}/reset-password`, {
      token,
      newPassword
    });
  }

  loadProfile(): Observable<UserProfile> {
    return this.http
      .get<UserProfile>(`${this.baseUrl}/me`, { withCredentials: true })
      .pipe(
        tap((user) => {
          this.userContext.setUser(user);
        })
      );
  }

  applySession(response: LoginResponse): void {
    this.tokenService.setExpiry(response.accessTokenExpiresAtUtc);
    this.userContext.setUser(response.user);
  }

  hasRole(...roles: string[]): boolean {
    const role = this.userContext.role();
    return !!role && roles.includes(role);
  }
}

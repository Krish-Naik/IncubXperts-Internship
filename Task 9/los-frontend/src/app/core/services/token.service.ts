import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TokenService {
  private expiresAt: Date | null = null;

  setExpiry(expiresAtUtc: string): void {
    this.expiresAt = new Date(expiresAtUtc);
  }

  getExpiresAt(): Date | null {
    return this.expiresAt;
  }

  isAccessTokenExpired(): boolean {
    if (!this.expiresAt) return true;
    return this.expiresAt.getTime() <= Date.now() + 30_000;
  }

  clear(): void {
    this.expiresAt = null;
  }

  hasSession(): boolean {
    return this.expiresAt !== null;
  }
}

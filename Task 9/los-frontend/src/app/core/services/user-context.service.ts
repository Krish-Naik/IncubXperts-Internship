import { Injectable, computed, signal } from '@angular/core';
import { UserProfile } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class UserContextService {
  private readonly userSignal = signal<UserProfile | null>(null);
  readonly user = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.userSignal());
  readonly role = computed(() => this.userSignal()?.role ?? null);

  setUser(user: UserProfile | null): void {
    this.userSignal.set(user);
  }
}

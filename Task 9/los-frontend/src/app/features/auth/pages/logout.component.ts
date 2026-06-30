import { Component, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-logout',
  standalone: true,
  template: `<p>Signing you out...</p>`
})
export class LogoutComponent {
  private readonly authService = inject(AuthService);

  constructor() {
    this.authService.logout().subscribe();
  }
}

import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RoleOption } from '../../../core/models/role.model';
import { UserManagementService } from '../services/user-management.service';

@Component({
  selector: 'app-role-selector',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <label>Role</label>
    <select [formControl]="control">
      <option value="">Select role</option>
      @for (role of roles(); track role.id) {
        <option [value]="role.id">{{ role.name }}</option>
      }
    </select>
  `
})
export class RoleSelectorComponent implements OnInit {
  @Input({ required: true }) control!: ReturnType<FormBuilder['control']>;
  private readonly userService = inject(UserManagementService);
  readonly roles = signal<RoleOption[]>([]);

  ngOnInit(): void {
    this.userService.getRoles().subscribe((roles) => this.roles.set(roles));
  }
}

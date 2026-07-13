import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { BranchOption } from '../../../core/models/branch.model';
import { RoleOption } from '../../../core/models/role.model';
import { CreateUserRequest, UpdateUserRequest } from '../../../core/models/user.model';
import { UserManagementService } from '../services/user-management.service';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <form [formGroup]="form" (ngSubmit)="onSubmit()" class="form-grid">
      <label>Employee ID</label>
      <input formControlName="employeeId" [readonly]="mode === 'edit'" />

      <label>Full name</label>
      <input formControlName="fullName" />

      <label>Email</label>
      <input formControlName="email" type="email" [readonly]="mode === 'edit'" />

      <label>Phone</label>
      <input formControlName="phone" />

      <label>Role</label>
      <select formControlName="roleId">
        <option value="">Select role</option>
        @for (role of roles(); track role.id) {
          <option [value]="role.id">{{ role.name }}</option>
        }
      </select>

      <label>Branch</label>
      <select formControlName="branchId">
        <option value="">No branch</option>
        @for (branch of branches(); track branch.id) {
          <option [value]="branch.id">{{ branch.name }}</option>
        }
      </select>

      <button type="submit" [disabled]="form.invalid">{{ submitLabel }}</button>
    </form>
  `,
  styles: [
    `
      .form-grid {
        display: grid;
        gap: 0.75rem;
        max-width: 520px;
      }
      input,
      select,
      button {
        padding: 0.75rem;
        border-radius: 8px;
        border: 1px solid #cbd5e0;
      }
      button {
        background: #1f7a8c;
        color: #fff;
        border: 0;
        justify-self: start;
      }
    `
  ]
})
export class UserFormComponent implements OnInit {
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() submitLabel = 'Save';
  @Input() initialValue?: {
    employeeId: string;
    fullName: string;
    email: string;
    phone: string;
    roleId: string;
    branchId?: string | null;
  };
  @Output() save = new EventEmitter<CreateUserRequest | UpdateUserRequest>();

  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserManagementService);
  readonly roles = signal<RoleOption[]>([]);
  readonly branches = signal<BranchOption[]>([]);

  readonly form = this.fb.nonNullable.group({
    employeeId: ['', Validators.required],
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', Validators.required],
    roleId: ['', Validators.required],
    branchId: ['']
  });

  ngOnInit(): void {
    this.userService.getRoles().subscribe((roles) => this.roles.set(roles));
    this.userService.getBranches().subscribe((branches) => this.branches.set(branches));

    if (this.initialValue) {
      this.form.patchValue({
        ...this.initialValue,
        branchId: this.initialValue.branchId ?? ''
      });
    }
  }

  onSubmit(): void {
    const value = this.form.getRawValue();
    if (this.mode === 'create') {
      this.save.emit({
        employeeId: value.employeeId,
        fullName: value.fullName,
        email: value.email,
        phone: value.phone,
        roleId: value.roleId,
        branchId: value.branchId || null
      });
      return;
    }

    this.save.emit({
      fullName: value.fullName,
      phone: value.phone,
      roleId: value.roleId,
      branchId: value.branchId || null
    });
  }
}

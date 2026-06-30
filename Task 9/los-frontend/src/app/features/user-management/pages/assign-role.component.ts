import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { PageHeaderComponent } from '../../../shared/components/page-header.component';
import { RoleOption } from '../../../core/models/role.model';
import { UserManagementService } from '../services/user-management.service';

@Component({
  selector: 'app-assign-role',
  standalone: true,
  imports: [ReactiveFormsModule, PageHeaderComponent],
  template: `
    <app-page-header title="Assign role" />
    <form [formGroup]="form" (ngSubmit)="submit()">
      <select formControlName="roleId">
        @for (role of roles(); track role.id) {
          <option [value]="role.id">{{ role.name }}</option>
        }
      </select>
      <button type="submit">Save role</button>
    </form>
  `
})
export class AssignRoleComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly userService = inject(UserManagementService);
  private readonly fb = inject(FormBuilder);
  readonly roles = signal<RoleOption[]>([]);
  readonly form = this.fb.nonNullable.group({ roleId: [''] });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.userService.getRoles().subscribe((roles) => this.roles.set(roles));
    this.userService.getUser(id).subscribe((user) => this.form.patchValue({ roleId: user.roleId }));
  }

  submit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.userService.assignRole(id, this.form.controls.roleId.value).subscribe();
  }
}

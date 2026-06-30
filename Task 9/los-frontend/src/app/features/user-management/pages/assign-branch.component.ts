import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { PageHeaderComponent } from '../../../shared/components/page-header.component';
import { BranchOption } from '../../../core/models/branch.model';
import { UserManagementService } from '../services/user-management.service';

@Component({
  selector: 'app-assign-branch',
  standalone: true,
  imports: [ReactiveFormsModule, PageHeaderComponent],
  template: `
    <app-page-header title="Assign branch" />
    <form [formGroup]="form" (ngSubmit)="submit()">
      <select formControlName="branchId">
        <option value="">No branch</option>
        @for (branch of branches(); track branch.id) {
          <option [value]="branch.id">{{ branch.name }}</option>
        }
      </select>
      <button type="submit">Save branch</button>
    </form>
  `
})
export class AssignBranchComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly userService = inject(UserManagementService);
  private readonly fb = inject(FormBuilder);
  readonly branches = signal<BranchOption[]>([]);
  readonly form = this.fb.nonNullable.group({ branchId: [''] });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.userService.getBranches().subscribe((branches) => this.branches.set(branches));
    this.userService.getUser(id).subscribe((user) =>
      this.form.patchValue({ branchId: user.branchId ?? '' })
    );
  }

  submit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    const branchId = this.form.controls.branchId.value || null;
    this.userService.assignBranch(id, branchId).subscribe();
  }
}

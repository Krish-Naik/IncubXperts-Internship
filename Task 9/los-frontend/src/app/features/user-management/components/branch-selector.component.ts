import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { BranchOption } from '../../../core/models/branch.model';
import { UserManagementService } from '../services/user-management.service';

@Component({
  selector: 'app-branch-selector',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <label>Branch</label>
    <select [formControl]="control">
      <option value="">No branch</option>
      @for (branch of branches(); track branch.id) {
        <option [value]="branch.id">{{ branch.name }} ({{ branch.code }})</option>
      }
    </select>
  `
})
export class BranchSelectorComponent implements OnInit {
  @Input({ required: true }) control!: ReturnType<FormBuilder['control']>;
  private readonly userService = inject(UserManagementService);
  readonly branches = signal<BranchOption[]>([]);

  ngOnInit(): void {
    this.userService.getBranches().subscribe((branches) => this.branches.set(branches));
  }
}

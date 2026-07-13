import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { BranchOption } from '../../../core/models/branch.model';
import { RoleOption } from '../../../core/models/role.model';
import {
  CreateUserRequest,
  InviteStatus,
  UpdateUserRequest,
  UserDetail,
  UserListItem
} from '../../../core/models/user.model';

@Injectable({ providedIn: 'root' })
export class UserManagementService {
  private readonly usersUrl = `${environment.apiUrl}/users`;
  private readonly rolesUrl = `${environment.apiUrl}/roles`;
  private readonly branchesUrl = `${environment.apiUrl}/branches`;

  constructor(private readonly http: HttpClient) {}

  getUsers(): Observable<UserListItem[]> {
    return this.http.get<UserListItem[]>(this.usersUrl);
  }

  getUser(id: string): Observable<UserDetail> {
    return this.http.get<UserDetail>(`${this.usersUrl}/${id}`);
  }

  createUser(request: CreateUserRequest): Observable<UserDetail> {
    return this.http.post<UserDetail>(this.usersUrl, request);
  }

  updateUser(id: string, request: UpdateUserRequest): Observable<UserDetail> {
    return this.http.put<UserDetail>(`${this.usersUrl}/${id}`, request);
  }

  activateUser(id: string): Observable<UserDetail> {
    return this.http.post<UserDetail>(`${this.usersUrl}/${id}/activate`, {});
  }

  deactivateUser(id: string): Observable<UserDetail> {
    return this.http.post<UserDetail>(`${this.usersUrl}/${id}/deactivate`, {});
  }

  assignRole(id: string, roleId: string): Observable<UserDetail> {
    return this.http.put<UserDetail>(`${this.usersUrl}/${id}/role`, { roleId });
  }

  assignBranch(id: string, branchId: string | null): Observable<UserDetail> {
    return this.http.put<UserDetail>(`${this.usersUrl}/${id}/branch`, { branchId });
  }

  getInviteStatus(id: string): Observable<InviteStatus> {
    return this.http.get<InviteStatus>(`${this.usersUrl}/${id}/invite-status`);
  }

  resendInvite(id: string): Observable<void> {
    return this.http.post<void>(`${this.usersUrl}/${id}/resend-invite`, {});
  }

  getRoles(): Observable<RoleOption[]> {
    return this.http.get<RoleOption[]>(this.rolesUrl);
  }

  getBranches(): Observable<BranchOption[]> {
    return this.http.get<BranchOption[]>(this.branchesUrl);
  }
}

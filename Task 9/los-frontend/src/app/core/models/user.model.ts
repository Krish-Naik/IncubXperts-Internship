export interface UserListItem {
  id: string;
  employeeId: string;
  fullName: string;
  email: string;
  phone: string;
  role: string;
  branchName?: string | null;
  status: string;
  createdAtUtc: string;
}

export interface UserDetail {
  id: string;
  employeeId: string;
  fullName: string;
  email: string;
  phone: string;
  roleId: string;
  role: string;
  branchId?: string | null;
  branchName?: string | null;
  status: string;
  mustChangePassword: boolean;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}

export interface CreateUserRequest {
  employeeId: string;
  fullName: string;
  email: string;
  phone: string;
  roleId: string;
  branchId?: string | null;
}

export interface UpdateUserRequest {
  fullName: string;
  phone: string;
  roleId: string;
  branchId?: string | null;
}

export interface InviteStatus {
  status: string;
  expiresAtUtc?: string | null;
}

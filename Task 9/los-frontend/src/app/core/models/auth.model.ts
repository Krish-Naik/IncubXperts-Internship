export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserProfile {
  id: string;
  employeeId: string;
  fullName: string;
  email: string;
  phone: string;
  role: string;
  branchId?: string | null;
  branchName?: string | null;
  status: string;
  mustChangePassword: boolean;
}

export interface LoginResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  user: UserProfile;
  landingRoute: string;
}

export interface MessageResponse {
  message: string;
}

export const ROLE_LANDING_ROUTES: Record<string, string> = {
  SystemAdministrator: '/admin/users',
  VerificationOfficer: '/verification',
  BranchManager: '/approvals',
  Customer: '/my-application',
  BrokerAgent: '/broker/leads'
};

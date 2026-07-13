export interface RegisterLeadRequest {
  fullName: string;
  email: string;
  phone: string;
}

export interface LeadResult {
  id: string;
  fullName: string;
  email: string;
  phone: string;
}

export interface ReferredApplicationRow {
  id: string;
  referenceNumber: string;
  status: string;
  commission: number | null;
}
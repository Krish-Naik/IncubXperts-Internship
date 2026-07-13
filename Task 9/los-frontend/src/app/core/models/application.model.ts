export enum LoanType { Home = 'Home', Personal = 'Personal', Auto = 'Auto', Business = 'Business' }
export enum ApplicationStatus {
  Draft = 'Draft',
  Submitted = 'Submitted',
  UnderReview = 'UnderReview',
  InfoRequested = 'InfoRequested',
  Verified = 'Verified',
  PendingSeniorApproval = 'PendingSeniorApproval',
  Approved = 'Approved',
  Rejected = 'Rejected',
  OfferAccepted = 'OfferAccepted',
  OfferDeclined = 'OfferDeclined',
  Disbursed = 'Disbursed'
}

export interface CoApplicant {
  id: string;
  fullName: string;
  pan: string;
  monthlyIncome: number;
}

export interface LoanApplication {
  id: string;
  referenceNumber: string;
  loanType: LoanType;
  requestedAmount: number;
  requestedTenureMonths: number;
  status: ApplicationStatus;
  createdAtUtc: string;
  approvedInterestRate?: number | null;
  approvedTenureMonths?: number | null;
  monthlyEmi?: number | null;
  rejectionReason?: string | null;
  infoRequestDetails?: string | null;
  infoResponseText?: string | null;
  coApplicants?: CoApplicant[];
}

export const LOAN_TYPE_FIELDS: Record<LoanType, { key: string; label: string }[]> = {
  [LoanType.Home]: [
    { key: 'propertyAddress', label: 'Property address' },
    { key: 'propertyValue', label: 'Estimated property value (₹)' },
    { key: 'employmentType', label: 'Employment type (Salaried / Self-employed)' }
  ],
  [LoanType.Auto]: [
    { key: 'vehicleMake', label: 'Vehicle make & model' },
    { key: 'vehiclePrice', label: 'On-road vehicle price (₹)' },
    { key: 'dealerName', label: 'Dealer name' }
  ],
  [LoanType.Personal]: [
    { key: 'purpose', label: 'Purpose of loan' },
    { key: 'employmentType', label: 'Employment type (Salaried / Self-employed)' }
  ],
  [LoanType.Business]: [
    { key: 'businessName', label: 'Business name' },
    { key: 'annualTurnover', label: 'Annual turnover (₹)' },
    { key: 'yearsInOperation', label: 'Years in operation' }
  ]
};
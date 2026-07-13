export interface ApprovalQueueItem {
    id: string;
    referenceNumber: string;
    customerName: string;
    loanType: string;
    requestedAmount: number;
    requestedTenureMonths: number;
    status: string;
  }
  
  export interface DisbursementQueueItem {
    id: string;
    referenceNumber: string;
    customerName: string;
    requestedAmount: number;
    approvedInterestRate: number;
    monthlyEmi: number;
  }
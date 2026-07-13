export interface KycQueueItem {
  id: string;
  loanApplicationId: string;
  referenceNumber: string;
  customerName: string;
  docType: string;
  originalFileName: string;
  uploadedByBroker: boolean;
  uploadedAtUtc: string;
}
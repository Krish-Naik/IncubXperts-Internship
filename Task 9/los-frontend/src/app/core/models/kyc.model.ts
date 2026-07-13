export interface KycQueueItem {
    id: string;
    referenceNumber: string;
    customerName: string;
    docType: string;
    originalFileName: string;
    uploadedByBroker: boolean;
  }
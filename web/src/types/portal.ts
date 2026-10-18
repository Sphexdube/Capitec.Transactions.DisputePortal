export type Customer = { customerId: number; displayName: string };
export type Transaction = {
  transactionId: string;
  merchant: string;
  description: string;
  occurredAt: string;
  amountMinor: string;
  currencyCode: string;
  state: string;
};
export type Page<T> = { items: T[]; nextCursor: string | null; pageSize: number };
export type Dispute = {
  disputeId: string;
  transaction: Transaction;
  reason: string;
  explanation?: string;
  status: string;
  createdAt: string;
  updatedAt: string;
};
export type HistoryEvent = {
  eventType: string;
  previousStatus: string | null;
  currentStatus: string;
  occurredAt: string;
  actor: string;
};
export type Confirmation = {
  disputeId: string;
  status: string;
  message: string | null;
  recordedAt: string | null;
};
export type SubmittedDispute = { disputeId: string; status: string; createdAt: string };
export type Envelope<T> = { data: T; correlationId?: string };

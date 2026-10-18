export const DISPUTE_REASONS = [
  'I do not recognise this transaction',
  'I was charged more than once',
  'The amount is incorrect',
  'I did not receive the goods or service',
  'Other transaction issue',
] as const;
export const STATUS_LABELS: Record<string, string> = {
  Submitted: 'Submitted',
  UnderReview: 'Under review',
  Resolved: 'Resolved',
  Rejected: 'Rejected',
  Pending: 'Pending',
  Recorded: 'Recorded',
  Posted: 'Completed',
  Completed: 'Completed',
};
export const EXPLANATION_MAX_LENGTH = 500;

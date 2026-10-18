namespace Capitec.Transactions.DisputePortal.Domain.Observability.Messages
{
    public static class ValidationMessages
    {
        public const string DisputeReasonRequired = "A dispute reason is required.";
        public const string DisputeReasonTooLong = "The dispute reason cannot exceed 100 characters.";
        public const string ExplanationTooLong = "The explanation cannot exceed 500 characters.";
        public const string TransactionRequired = "A valid transaction is required.";
        public const string CustomerRequired = "A valid customer is required.";
        public const string IdempotencyKeyRequired = "A valid idempotency key is required.";
        public const string PageSizeOutOfRange = "Page size must be between 1 and 50.";
        public const string EndDateBeforeStartDate = "End date must not be earlier than start date.";
        public const string TransactionCursorInvalid = "The transaction cursor is invalid.";
        public const string DisputeCursorInvalid = "The dispute cursor is invalid.";
        public const string CurrencyCodeInvalid = "Currency code must be a three-letter ISO code.";
    }
}

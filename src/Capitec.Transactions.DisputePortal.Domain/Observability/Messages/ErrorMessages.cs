namespace Capitec.Transactions.DisputePortal.Domain.Observability.Messages
{
    public static class ErrorMessages
    {
        public const string MethodFailed = "The method failed.";
        public const string ApiRequestFailed = "The API request failed. CorrelationId: {CorrelationId}";
        public const string OutboxProcessingFailed = "Confirmation outbox processing failed.";
        public const string SaveContextFailed = "The dispute data could not be saved.";
        public const string UniqueConstraintViolated = "A concurrent request saved the same data first.";
        public const string TransactionNotFound = "The transaction was not found.";
        public const string DisputeNotFound = "The dispute was not found.";
        public const string ActiveDisputeExists = "An active dispute already exists for this transaction.";
        public const string IdempotencyConflict = "The idempotency key was already used with a different request.";
        public const string IdempotencyRecordMissingDispute = "The idempotency record references a missing dispute.";
        public const string DisputeMissingTransaction = "The dispute references a missing transaction.";
        public const string DemoCustomerNotFound = "The demo customer was not found.";
        public const string DatabaseTransactionAlreadyActive = "A database transaction is already active.";
        public const string DatabaseTransactionNotActive = "No database transaction is active.";
        public const string OutboxUnsupportedEventType = "The outbox contains an unsupported event type.";
        public const string OutboxInvalidPayload = "The outbox event payload is invalid.";
        public const string OutboxMissingDispute = "The outbox event references a missing dispute.";
        public const string DisputeTransitionNotAllowedFormat = "A dispute cannot move from {0} to {1}.";
        public const string MoneyCurrencyMismatch = "Money values with different currencies cannot be combined.";
        public const string ConnectionStringMissingFormat = "The connection string, {0}, must be configured.";
        public const string ConfigurationSectionMissingFormat = "The configuration section, {0}, must be configured.";
        public const string SigningKeyTooShort = "The authentication signing key must contain at least 32 bytes.";
        public const string CustomerSessionRequired = "A valid customer session is required.";
        public const string RequestInvalid = "The request is invalid.";
        public const string FieldInvalidFormat = "{0}: Invalid value.";
        public const string FieldErrorFormat = "{0}: {1}";
        public const string TransactionNotFoundDetail = "Transaction not found.";
        public const string DisputeNotFoundDetail = "Dispute not found.";
        public const string IdempotencyConflictDetail = "The idempotency key was used with a different request.";
        public const string ConcurrentRequestDetail = "Another request changed this data at the same time. Refresh and try again.";
        public const string PersistenceUnavailableDetail = "The request could not be saved. Retry with the same idempotency key.";
        public const string InternalErrorDetail = "An unexpected error occurred.";
        public const string InvalidDemoCustomerDetail = "Select a valid demo customer.";
        public const string ResourceNotFoundDetail = "The requested resource was not found.";
    }
}

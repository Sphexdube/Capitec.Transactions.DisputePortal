namespace Capitec.Transactions.DisputePortal.Domain.Errors
{
    public static class ApiErrorCodes
    {
        public const string Unauthenticated = "UNAUTHENTICATED";
        public const string ValidationError = "VALIDATION_ERROR";
        public const string NotFound = "NOT_FOUND";
        public const string ActiveDisputeExists = "ACTIVE_DISPUTE_EXISTS";
        public const string IdempotencyConflict = "IDEMPOTENCY_CONFLICT";
        public const string ConcurrentRequest = "CONCURRENT_REQUEST";
        public const string PersistenceUnavailable = "PERSISTENCE_UNAVAILABLE";
        public const string InternalError = "INTERNAL_ERROR";
        public const string InvalidDemoCustomer = "INVALID_DEMO_CUSTOMER";
    }
}

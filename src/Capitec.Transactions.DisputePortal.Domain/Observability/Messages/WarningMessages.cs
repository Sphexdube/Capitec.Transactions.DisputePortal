namespace Capitec.Transactions.DisputePortal.Domain.Observability.Messages
{
    public static class WarningMessages
    {
        public const string OutboxMessageRescheduled = "The outbox message, {EventId}, could not be processed and was rescheduled. Reason: {Reason}";
        public const string ConcurrentSubmissionRetried = "A concurrent dispute submission committed first; the submission was retried.";
        public const string BearerAuthenticationFailed = "Bearer authentication failed. ExceptionType: {ExceptionType}";
    }
}

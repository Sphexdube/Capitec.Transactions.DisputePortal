namespace Capitec.Transactions.DisputePortal.Domain.Observability.Messages
{
    public static class InfoMessages
    {
        public const string MethodStarted = "The method has started.";
        public const string MethodCompleted = "The method has completed.";
        public const string DisputeSubmissionReceived = "Your dispute submission has been received.";
        public const string DisputeSubmitted = "Dispute {DisputeId} was submitted.";
        public const string ApiRequestRejected = "The API request was rejected. StatusCode: {StatusCode}, ErrorCode: {ErrorCode}, CorrelationId: {CorrelationId}";
        public const string ConfirmationBatchProcessed = "Processed {Count} confirmation outbox messages.";
    }
}

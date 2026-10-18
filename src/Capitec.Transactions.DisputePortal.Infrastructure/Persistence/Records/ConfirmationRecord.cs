namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records
{
    public sealed class ConfirmationRecord
    {
        public long DisputeId { get; private set; }

        public Guid OutboxEventId { get; private set; }

        public string Message { get; private set; } = default!;

        public DateTimeOffset CreatedAt { get; private set; }

        public ConfirmationRecord(long disputeId, Guid outboxEventId, string message, DateTimeOffset createdAt)
        {
            DisputeId = disputeId;
            OutboxEventId = outboxEventId;
            Message = message;
            CreatedAt = createdAt;
        }
    }
}
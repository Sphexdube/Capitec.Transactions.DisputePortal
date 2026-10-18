namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records
{
    public sealed class IdempotencyRecord
    {
        public long CustomerId { get; private set; }

        public string Operation { get; private set; } = default!;

        public string IdempotencyKey { get; private set; } = default!;

        public string RequestHash { get; private set; } = default!;

        public Guid DisputeExternalId { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public IdempotencyRecord(
            long customerId,
            string operation,
            string idempotencyKey,
            string requestHash,
            Guid disputeExternalId,
            DateTimeOffset createdAt)
        {
            CustomerId = customerId;
            Operation = operation;
            IdempotencyKey = idempotencyKey;
            RequestHash = requestHash;
            DisputeExternalId = disputeExternalId;
            CreatedAt = createdAt;
        }
    }
}
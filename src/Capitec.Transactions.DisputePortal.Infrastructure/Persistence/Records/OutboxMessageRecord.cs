namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records
{
    public sealed class OutboxMessageRecord
    {
        public Guid EventId { get; private set; }

        public string EventType { get; private set; } = default!;

        public string Payload { get; private set; } = default!;

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset NextAttemptAt { get; private set; }

        public OutboxMessageRecord(Guid eventId, string eventType, string payload, DateTimeOffset createdAt)
        {
            EventId = eventId;
            EventType = eventType;
            Payload = payload;
            CreatedAt = createdAt;
            NextAttemptAt = createdAt;
        }
    }
}
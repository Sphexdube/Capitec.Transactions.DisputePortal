namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records
{
    public sealed class DisputeHistoryRecord
    {
        public long DisputeId { get; private set; }

        public string EventType { get; private set; } = default!;

        public string? PreviousStatus { get; private set; }

        public string CurrentStatus { get; private set; } = default!;

        public DateTimeOffset OccurredAt { get; private set; }

        public string Actor { get; private set; } = default!;

        public DisputeHistoryRecord(
            long disputeId,
            string eventType,
            string? previousStatus,
            string currentStatus,
            DateTimeOffset occurredAt,
            string actor)
        {
            DisputeId = disputeId;
            EventType = eventType;
            PreviousStatus = previousStatus;
            CurrentStatus = currentStatus;
            OccurredAt = occurredAt;
            Actor = actor;
        }
    }
}
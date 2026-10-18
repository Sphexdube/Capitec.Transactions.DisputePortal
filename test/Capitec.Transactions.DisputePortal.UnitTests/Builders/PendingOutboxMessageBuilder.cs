using System.Text.Json;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Outbox;

namespace Capitec.Transactions.DisputePortal.UnitTests.Builders
{
    public sealed class PendingOutboxMessageBuilder
    {
        private Guid _eventId = Guid.NewGuid();
        private Guid _disputeId = Guid.NewGuid();
        private string _eventType = OutboxEventTypes.DisputeSubmittedV1;
        private string? _payload;

        public PendingOutboxMessageBuilder WithDisputeId(Guid disputeId)
        {
            _disputeId = disputeId;
            return this;
        }

        public PendingOutboxMessageBuilder WithEventType(string eventType)
        {
            _eventType = eventType;
            return this;
        }

        public PendingOutboxMessageBuilder WithPayload(string payload)
        {
            _payload = payload;
            return this;
        }

        public PendingOutboxMessage Build()
        {
            DateTimeOffset createdAt = new(2026, 10, 18, 10, 0, 0, TimeSpan.Zero);
            string payload = _payload ?? JsonSerializer.Serialize(new DisputeSubmittedEvent(_eventId, _disputeId, Guid.NewGuid(), createdAt));

            return new PendingOutboxMessage(_eventId, _eventType, payload, createdAt);
        }
    }
}

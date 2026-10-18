namespace Capitec.Transactions.DisputePortal.Domain.Outbox
{
    public sealed record PendingOutboxMessage(
        Guid EventId,
        string EventType,
        string Payload,
        DateTimeOffset CreatedAt);
}

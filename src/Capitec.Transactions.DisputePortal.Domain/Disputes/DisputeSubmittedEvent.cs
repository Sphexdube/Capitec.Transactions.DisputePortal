namespace Capitec.Transactions.DisputePortal.Domain.Disputes
{
    public sealed record DisputeSubmittedEvent(
        Guid EventId,
        Guid DisputeId,
        Guid TransactionId,
        DateTimeOffset SubmittedAt);
}

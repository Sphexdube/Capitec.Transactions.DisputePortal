namespace Capitec.Transactions.DisputePortal.Domain.Disputes
{
    public sealed record DisputeConfirmation(
        long DisputeId,
        Guid OutboxEventId,
        string Message,
        DateTimeOffset CreatedAt);
}

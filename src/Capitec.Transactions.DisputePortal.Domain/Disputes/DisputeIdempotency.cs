namespace Capitec.Transactions.DisputePortal.Domain.Disputes
{
    public sealed record DisputeIdempotency(
        long CustomerId,
        string Operation,
        string Key,
        string RequestHash,
        Guid DisputeExternalId,
        DateTimeOffset CreatedAt);
}

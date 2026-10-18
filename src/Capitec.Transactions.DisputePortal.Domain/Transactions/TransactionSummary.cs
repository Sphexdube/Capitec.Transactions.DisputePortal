namespace Capitec.Transactions.DisputePortal.Domain.Transactions
{
    public sealed record TransactionSummary(
        long Id,
        Guid ExternalId,
        string Merchant,
        string Description,
        DateTimeOffset OccurredAt,
        long AmountMinor,
        string CurrencyCode,
        string State);
}

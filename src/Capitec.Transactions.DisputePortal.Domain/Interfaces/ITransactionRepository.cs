using Capitec.Transactions.DisputePortal.Domain.Transactions;

namespace Capitec.Transactions.DisputePortal.Domain.Interfaces
{
    public interface ITransactionRepository
    {
        public Task<long?> GetOwnedTransactionIdAsync(Guid externalId, long customerId, CancellationToken cancellationToken);

        public Task<TransactionSummary?> GetOwnedSummaryByExternalIdAsync(Guid externalId, long customerId, CancellationToken cancellationToken);

        public Task<TransactionSummary?> GetOwnedSummaryAsync(long transactionId, long customerId, CancellationToken cancellationToken);

        public Task<IReadOnlyList<TransactionSummary>> GetPageAsync(
            long customerId,
            DateTimeOffset? beforeOccurredAt,
            long? beforeId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            int limit,
            CancellationToken cancellationToken);

        public Task<IReadOnlyList<TransactionSummary>> GetOwnedSummariesAsync(
            long customerId,
            IReadOnlyCollection<long> transactionIds,
            CancellationToken cancellationToken);
    }
}

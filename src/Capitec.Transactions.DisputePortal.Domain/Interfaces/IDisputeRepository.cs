using Capitec.Transactions.DisputePortal.Domain.Disputes;

namespace Capitec.Transactions.DisputePortal.Domain.Interfaces
{
    public interface IDisputeRepository
    {
        public Task<Dispute?> GetByExternalIdAsync(Guid externalId, long customerId, CancellationToken cancellationToken);

        public Task<long?> GetIdByExternalIdAsync(Guid externalId, CancellationToken cancellationToken);

        public Task<bool> HasActiveForTransactionAsync(long customerId, long transactionId, CancellationToken cancellationToken);

        public Task<IReadOnlyList<Dispute>> GetCustomerPageAsync(
            long customerId,
            DateTimeOffset? beforeCreatedAt,
            long? beforeId,
            int limit,
            CancellationToken cancellationToken);

        public void Add(Dispute dispute);
    }
}

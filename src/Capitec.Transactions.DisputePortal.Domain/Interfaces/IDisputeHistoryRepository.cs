using Capitec.Transactions.DisputePortal.Domain.Disputes;

namespace Capitec.Transactions.DisputePortal.Domain.Interfaces
{
    public interface IDisputeHistoryRepository
    {
        public void Add(DisputeHistoryEntry historyEntry);

        public Task<IReadOnlyList<DisputeHistoryEntry>> GetByDisputeIdAsync(long disputeId, CancellationToken cancellationToken);
    }
}

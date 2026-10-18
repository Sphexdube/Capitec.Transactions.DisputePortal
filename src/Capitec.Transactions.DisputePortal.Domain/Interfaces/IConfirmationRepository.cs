using Capitec.Transactions.DisputePortal.Domain.Disputes;

namespace Capitec.Transactions.DisputePortal.Domain.Interfaces
{
    public interface IConfirmationRepository
    {
        public Task<DisputeConfirmation?> GetByDisputeIdAsync(long disputeId, CancellationToken cancellationToken);

        public void Add(DisputeConfirmation confirmation);
    }
}

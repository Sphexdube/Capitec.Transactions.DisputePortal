using Capitec.Transactions.DisputePortal.Domain.Disputes;

namespace Capitec.Transactions.DisputePortal.Domain.Interfaces
{
    public interface IDisputeIdempotencyRepository
    {
        public Task<DisputeIdempotency?> GetAsync(
            long customerId,
            string operation,
            string key,
            CancellationToken cancellationToken);

        public void Add(DisputeIdempotency idempotency);
    }
}

using Capitec.Transactions.DisputePortal.Domain.Outbox;

namespace Capitec.Transactions.DisputePortal.Domain.Interfaces
{
    public interface IOutboxRepository
    {
        public void Add(PendingOutboxMessage message);

        public Task<IReadOnlyList<PendingOutboxMessage>> LockDueAsync(
            DateTimeOffset dueAt,
            int batchSize,
            CancellationToken cancellationToken);

        public Task<int> RescheduleAsync(Guid eventId, DateTimeOffset nextAttemptAt, CancellationToken cancellationToken);

        public Task<int> DeleteAsync(IReadOnlyCollection<Guid> eventIds, CancellationToken cancellationToken);
    }
}

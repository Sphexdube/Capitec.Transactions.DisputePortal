namespace Capitec.Transactions.DisputePortal.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        public ICustomerRepository Customers { get; }

        public IDisputeRepository Disputes { get; }

        public ITransactionRepository Transactions { get; }

        public IDisputeIdempotencyRepository Idempotency { get; }

        public IDisputeHistoryRepository DisputeHistory { get; }

        public IOutboxRepository Outbox { get; }

        public IConfirmationRepository Confirmations { get; }

        public Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            CancellationToken cancellationToken = default);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

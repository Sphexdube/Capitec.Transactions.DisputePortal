namespace Capitec.Transactions.DisputePortal.Domain.Interfaces
{
    public interface IDisputeConfirmationProcessor
    {
        public Task<int> ProcessBatchAsync(int batchSize, CancellationToken cancellationToken);
    }
}

namespace Capitec.Transactions.DisputePortal.Domain.Outbox
{
    public static class OutboxLimits
    {
        public static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(1);
    }
}

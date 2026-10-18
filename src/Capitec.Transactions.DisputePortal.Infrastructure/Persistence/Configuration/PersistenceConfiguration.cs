namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Configuration
{
    public sealed record PersistenceConfiguration
    {
        public const string SectionName = "Persistence";
        public const string ConnectionStringName = "DisputePortal";

        public int MaxRetryCount { get; init; } = 3;

        public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromSeconds(5);

        public TimeSpan CommandTimeout { get; init; } = TimeSpan.FromSeconds(30);
    }
}

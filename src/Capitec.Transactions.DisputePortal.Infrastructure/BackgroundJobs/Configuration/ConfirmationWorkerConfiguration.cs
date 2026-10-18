namespace Capitec.Transactions.DisputePortal.Infrastructure.BackgroundJobs.Configuration
{
    public sealed record ConfirmationWorkerConfiguration
    {
        public const string SectionName = "ConfirmationWorker";

        public bool Enabled { get; init; }

        public int BatchSize { get; init; } = 50;

        public TimeSpan IdleDelay { get; init; } = TimeSpan.FromSeconds(1);

        public TimeSpan FailureDelay { get; init; } = TimeSpan.FromSeconds(5);
    }
}

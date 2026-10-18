using Capitec.Transactions.DisputePortal.Api.Authentication;
using Capitec.Transactions.DisputePortal.Infrastructure.BackgroundJobs.Configuration;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Configuration;

namespace Capitec.Transactions.DisputePortal.Api.Dependencies
{
    public sealed record ApiConfiguration
    {
        public required AuthenticationConfiguration Authentication { get; init; }

        public required DemoAuthenticationConfiguration DemoAuthentication { get; init; }

        public required PersistenceConfiguration Persistence { get; init; }

        public required ConfirmationWorkerConfiguration ConfirmationWorker { get; init; }
    }
}

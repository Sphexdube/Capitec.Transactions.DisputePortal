using Capitec.Transactions.DisputePortal.Api.Dependencies;

namespace Capitec.Transactions.DisputePortal.Api.Authentication
{
    public sealed record DemoAuthenticationConfiguration
    {
        public const string SectionName = "DemoAuthentication";

        public bool Enabled { get; init; }

        public bool IsAvailableIn(IHostEnvironment environment)
        {
            return Enabled && ApiEnvironments.IsLocalOrDevelopment(environment);
        }
    }
}

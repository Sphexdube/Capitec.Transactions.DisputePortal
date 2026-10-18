using Microsoft.Extensions.DependencyInjection;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Health.Extensions
{
    public static class HealthChecksBuilderExtensions
    {
        public const string DatabaseCheckName = "database";
        public const string ReadyTag = "ready";

        public static IHealthChecksBuilder AddPersistenceChecks(this IHealthChecksBuilder builder)
        {
            return builder.AddCheck<DatabaseHealthCheck>(DatabaseCheckName, tags: [ReadyTag]);
        }
    }
}

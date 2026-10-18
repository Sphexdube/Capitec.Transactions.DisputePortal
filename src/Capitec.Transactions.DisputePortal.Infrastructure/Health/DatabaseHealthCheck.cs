using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Health
{
    internal sealed class DatabaseHealthCheck(IDbContextFactory<DisputePortalDbContext> contextFactory) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            await using (DisputePortalDbContext dbContext = await contextFactory.CreateDbContextAsync(cancellationToken))
            {
                bool canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

                return canConnect ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
            }
        }
    }
}

using Capitec.Transactions.DisputePortal.Infrastructure.Health;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Configuration;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Context;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Capitec.Transactions.DisputePortal.IntegrationTests.Infrastructure.Health
{
    public sealed class DatabaseHealthCheckTests
    {
        [Test]
        public async Task CheckHealthAsync_WhenDatabaseIsReachable_ShouldReturnHealthy()
        {
            DatabaseHealthCheck check = new(PostgresDatabaseFixture.ContextFactory);

            HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
        }

        [Test]
        public async Task CheckHealthAsync_WhenDatabaseIsUnreachable_ShouldReturnUnhealthy()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DisputePortal"] = "Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=1"
                })
                .Build();
            ServiceCollection services = new();
            services.AddPersistence(configuration, new PersistenceConfiguration { MaxRetryCount = 0 });

            using (ServiceProvider provider = services.BuildServiceProvider())
            {
                DatabaseHealthCheck check = new(provider.GetRequiredService<IDbContextFactory<DisputePortalDbContext>>());

                HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());

                Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
            }
        }
    }
}

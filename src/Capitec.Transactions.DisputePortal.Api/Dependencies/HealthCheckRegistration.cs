using Capitec.Transactions.DisputePortal.Infrastructure.Health.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Capitec.Transactions.DisputePortal.Api.Dependencies
{
    public static class HealthCheckRegistration
    {
        public const string LivePath = "/api/v1/health/live";
        public const string ReadyPath = "/api/v1/health/ready";

        public static void Register(IServiceCollection services)
        {
            services.AddHealthChecks().AddPersistenceChecks();
        }

        public static void Map(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapHealthChecks(LivePath, new HealthCheckOptions { Predicate = _ => false })
                .AllowAnonymous();
            endpoints
                .MapHealthChecks(ReadyPath, new HealthCheckOptions
                {
                    Predicate = registration => registration.Tags.Contains(HealthChecksBuilderExtensions.ReadyTag)
                })
                .AllowAnonymous();
        }
    }
}

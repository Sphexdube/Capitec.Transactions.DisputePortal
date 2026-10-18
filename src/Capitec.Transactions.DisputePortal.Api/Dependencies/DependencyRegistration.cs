using Capitec.Transactions.DisputePortal.Api.Authentication;
using Capitec.Transactions.DisputePortal.Application.Extensions;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Infrastructure.BackgroundJobs.Extensions;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Extensions;

namespace Capitec.Transactions.DisputePortal.Api.Dependencies
{
    public static class DependencyRegistration
    {
        public static void Register(IServiceCollection services, IConfiguration configuration, ApiConfiguration apiConfiguration)
        {
            ArgumentNullException.ThrowIfNull(apiConfiguration);

            services
                .AddApplication()
                .AddPersistence(configuration, apiConfiguration.Persistence)
                .AddBackgroundJobs(apiConfiguration.ConfirmationWorker);

            services.AddSingleton(apiConfiguration.Authentication);
            services.AddSingleton(apiConfiguration.DemoAuthentication);
            services.AddSingleton<IDemoTokenIssuer, JwtDemoTokenIssuer>();
        }
    }
}

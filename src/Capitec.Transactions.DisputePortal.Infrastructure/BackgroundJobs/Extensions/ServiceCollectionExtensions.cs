using Capitec.Transactions.DisputePortal.Infrastructure.BackgroundJobs.Configuration;
using Capitec.Transactions.DisputePortal.Infrastructure.BackgroundJobs.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace Capitec.Transactions.DisputePortal.Infrastructure.BackgroundJobs.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBackgroundJobs(
            this IServiceCollection services,
            ConfirmationWorkerConfiguration confirmationWorkerConfiguration)
        {
            ArgumentNullException.ThrowIfNull(confirmationWorkerConfiguration);

            services.AddSingleton(confirmationWorkerConfiguration);

            if (confirmationWorkerConfiguration.Enabled)
            {
                services.AddHostedService<DisputeConfirmationWorker>();
            }

            return services;
        }
    }
}

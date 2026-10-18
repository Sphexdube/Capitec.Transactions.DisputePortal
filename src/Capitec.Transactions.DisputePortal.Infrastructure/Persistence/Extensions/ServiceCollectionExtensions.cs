using System.Globalization;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Configuration;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Context;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration,
            PersistenceConfiguration persistenceConfiguration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(persistenceConfiguration);

            string? connectionString = configuration.GetConnectionString(PersistenceConfiguration.ConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    ErrorMessages.ConnectionStringMissingFormat,
                    PersistenceConfiguration.ConnectionStringName));
            }

            services.AddSingleton(persistenceConfiguration);
            services.AddDbContextFactory<DisputePortalDbContext>(
                options => options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(
                        persistenceConfiguration.MaxRetryCount,
                        persistenceConfiguration.MaxRetryDelay,
                        errorCodesToAdd: null);
                    npgsql.CommandTimeout((int)persistenceConfiguration.CommandTimeout.TotalSeconds);
                }),
                ServiceLifetime.Singleton);

            return services.AddTransient<IUnitOfWorkFactory<IUnitOfWork>, UnitOfWorkFactory>();
        }
    }
}

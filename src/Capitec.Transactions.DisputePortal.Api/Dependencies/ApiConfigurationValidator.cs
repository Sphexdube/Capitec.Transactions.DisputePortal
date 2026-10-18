using System.Text;
using Capitec.Transactions.DisputePortal.Api.Authentication;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Capitec.Transactions.DisputePortal.Infrastructure.BackgroundJobs.Configuration;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Configuration;

namespace Capitec.Transactions.DisputePortal.Api.Dependencies
{
    public static class ApiConfigurationValidator
    {
        public static ApiConfiguration Validate(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            AuthenticationConfiguration authentication = Bind<AuthenticationConfiguration>(configuration, AuthenticationConfiguration.SectionName);

            if (string.IsNullOrEmpty(authentication.SigningKey)
                || Encoding.UTF8.GetByteCount(authentication.SigningKey) < AuthenticationConfiguration.MinimumSigningKeyBytes)
            {
                throw new InvalidOperationException(ErrorMessages.SigningKeyTooShort);
            }

            return new ApiConfiguration
            {
                Authentication = authentication,
                DemoAuthentication = Bind<DemoAuthenticationConfiguration>(configuration, DemoAuthenticationConfiguration.SectionName),
                Persistence = Bind<PersistenceConfiguration>(configuration, PersistenceConfiguration.SectionName),
                ConfirmationWorker = Bind<ConfirmationWorkerConfiguration>(configuration, ConfirmationWorkerConfiguration.SectionName)
            };
        }

        private static T Bind<T>(IConfiguration configuration, string sectionName)
            where T : new()
        {
            return configuration.GetSection(sectionName).Get<T>() ?? new T();
        }
    }
}

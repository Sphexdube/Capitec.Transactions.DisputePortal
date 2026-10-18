using Capitec.Transactions.DisputePortal.Application.Extensions;
using Capitec.Transactions.DisputePortal.Application.Handlers;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Application.Validators.V1;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Extensions
{
    public sealed class ServiceCollectionExtensionsTests
    {
        [Test]
        public void AddApplication_WhenCalled_ShouldResolveHandlersAndValidators()
        {
            ServiceCollection services = new();
            services.AddLogging();
            services.AddSingleton(Substitute.For<IUnitOfWorkFactory<IUnitOfWork>>());
            services.AddSingleton(Substitute.For<IDemoTokenIssuer>());

            services.AddApplication();

            using (ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true }))
            {
                using (IServiceScope scope = provider.CreateScope())
                {
                    Assert.That(scope.ServiceProvider.GetRequiredService<IRequestHandler<SubmitDisputeRequest, SubmitDisputeResponse>>(), Is.Not.Null);
                    Assert.That(scope.ServiceProvider.GetRequiredService<IValidator<SubmitDisputeRequest>>(), Is.InstanceOf<SubmitDisputeRequestValidator>());
                    Assert.That(scope.ServiceProvider.GetRequiredService<IDisputeConfirmationProcessor>(), Is.Not.Null);
                    Assert.That(scope.ServiceProvider.GetRequiredService<TimeProvider>(), Is.SameAs(TimeProvider.System));
                }
            }
        }
    }
}

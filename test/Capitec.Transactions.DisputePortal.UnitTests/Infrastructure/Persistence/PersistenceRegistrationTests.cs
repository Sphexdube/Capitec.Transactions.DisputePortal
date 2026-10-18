using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Configuration;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Capitec.Transactions.DisputePortal.UnitTests.Infrastructure.Persistence
{
    public sealed class PersistenceRegistrationTests
    {
        [Test]
        public void AddPersistence_WhenConnectionStringIsSet_ShouldRegisterUnitOfWorkFactory()
        {
            ServiceCollection services = new();

            services.AddPersistence(Build("Host=localhost;Database=unused"), new PersistenceConfiguration());

            using (ServiceProvider provider = services.BuildServiceProvider())
            {
                Assert.That(provider.GetRequiredService<IUnitOfWorkFactory<IUnitOfWork>>(), Is.Not.Null);
            }
        }

        [Test]
        public void AddPersistence_WhenConnectionStringIsMissing_ShouldThrowInvalidOperationException()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => new ServiceCollection().AddPersistence(Build(null), new PersistenceConfiguration()));

            Assert.That(exception.Message, Is.EqualTo("The connection string, DisputePortal, must be configured."));
        }

        [Test]
        public void AddPersistence_WhenArgumentsAreNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddPersistence(null!, new PersistenceConfiguration()));
            Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddPersistence(Build("Host=x"), null!));
        }


        private static IConfiguration Build(string? connectionString)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:DisputePortal"] = connectionString })
                .Build();
        }
    }
}

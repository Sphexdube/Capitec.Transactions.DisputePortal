using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Configuration;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Context;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Capitec.Transactions.DisputePortal.IntegrationTests
{
    [SetUpFixture]
    public sealed class PostgresDatabaseFixture
    {
        private const string MigrationsDirectory = "src/dbDisputePortal/up";
        private const string DatabaseName = "dispute_portal";

        private static PostgreSqlContainer? _container;
        private static ServiceProvider? _services;

        public static string ConnectionString
        {
            get
            {
                return _container?.GetConnectionString() ?? throw new InvalidOperationException("The database fixture has not started.");
            }
        }

        public static IDbContextFactory<DisputePortalDbContext> ContextFactory
        {
            get
            {
                return Services.GetRequiredService<IDbContextFactory<DisputePortalDbContext>>();
            }
        }

        public static IUnitOfWorkFactory<IUnitOfWork> UnitOfWorkFactory
        {
            get
            {
                return Services.GetRequiredService<IUnitOfWorkFactory<IUnitOfWork>>();
            }
        }

        private static ServiceProvider Services
        {
            get
            {
                return _services ?? throw new InvalidOperationException("The database fixture has not started.");
            }
        }

        [OneTimeSetUp]
        public async Task StartAsync()
        {
            _container = new PostgreSqlBuilder("postgres:16-alpine").WithDatabase(DatabaseName).Build();
            await _container.StartAsync();
            await ApplyMigrationsAsync();

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:DisputePortal"] = ConnectionString })
                .Build();
            ServiceCollection services = new();
            services.AddPersistence(configuration, new PersistenceConfiguration());
            _services = services.BuildServiceProvider();
        }

        [OneTimeTearDown]
        public async Task StopAsync()
        {
            await ApiFactoryFixture.DisposeAsync();

            if (_services is not null)
            {
                await _services.DisposeAsync();
            }

            if (_container is not null)
            {
                await _container.DisposeAsync();
            }
        }

        private static async Task ApplyMigrationsAsync()
        {
            await using (NpgsqlConnection connection = new(ConnectionString))
            {
                await connection.OpenAsync();

                foreach (string script in Directory.GetFiles(ResolveMigrationsPath(), "*.sql").OrderBy(path => path, StringComparer.Ordinal))
                {
                    await using (NpgsqlCommand command = new(await File.ReadAllTextAsync(script), connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        private static string ResolveMigrationsPath()
        {
            DirectoryInfo? directory = new(AppContext.BaseDirectory);

            while (directory is not null)
            {
                string candidate = Path.Combine(directory.FullName, MigrationsDirectory);

                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException(MigrationsDirectory);
        }
    }
}

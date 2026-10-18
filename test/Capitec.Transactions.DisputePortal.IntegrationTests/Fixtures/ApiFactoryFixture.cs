using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Capitec.Transactions.DisputePortal.IntegrationTests
{
    public static class ApiFactoryFixture
    {
        public const string SigningKey = "integration-test-signing-key-at-least-32-bytes-long";

        private static readonly Lazy<WebApplicationFactory<Program>> LazyFactory = new(Create);

        public static WebApplicationFactory<Program> Factory
        {
            get
            {
                return LazyFactory.Value;
            }
        }

        public static async Task DisposeAsync()
        {
            if (LazyFactory.IsValueCreated)
            {
                await LazyFactory.Value.DisposeAsync();
            }
        }

        private static WebApplicationFactory<Program> Create()
        {
            return new WebApplicationFactory<Program>().WithWebHostBuilder(webHostBuilder =>
            {
                webHostBuilder.UseEnvironment("Development");
                webHostBuilder.UseSetting("ConnectionStrings:DisputePortal", PostgresDatabaseFixture.ConnectionString);
                webHostBuilder.UseSetting("Authentication:SigningKey", SigningKey);
                webHostBuilder.UseSetting("DemoAuthentication:Enabled", "true");
                webHostBuilder.UseSetting("ConfirmationWorker:Enabled", "true");
                webHostBuilder.UseSetting("ConfirmationWorker:IdleDelay", "00:00:00.100");
            });
        }
    }
}

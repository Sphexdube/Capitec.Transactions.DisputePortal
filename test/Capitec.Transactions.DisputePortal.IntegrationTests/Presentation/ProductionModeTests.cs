using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Capitec.Transactions.DisputePortal.IntegrationTests.Presentation
{
    public sealed class ProductionModeTests
    {
        private WebApplicationFactory<Program> _factory = null!;

        [OneTimeSetUp]
        public void StartProductionHost()
        {
            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(webHostBuilder =>
            {
                webHostBuilder.UseEnvironment("Production");
                webHostBuilder.UseSetting("ConnectionStrings:DisputePortal", PostgresDatabaseFixture.ConnectionString);
                webHostBuilder.UseSetting("Authentication:SigningKey", ApiFactoryFixture.SigningKey);
                webHostBuilder.UseSetting("DemoAuthentication:Enabled", "true");
                webHostBuilder.UseSetting("ConfirmationWorker:Enabled", "false");
            });
        }

        [OneTimeTearDown]
        public async Task StopProductionHost()
        {
            await _factory.DisposeAsync();
        }

        [TestCase("/swagger/v1/swagger.json", HttpStatusCode.Unauthorized)]
        [TestCase("/api/v1/demo-session", HttpStatusCode.NotFound)]
        [TestCase("/api/v1/health/ready", HttpStatusCode.OK)]
        public async Task Request_WhenRunningInProduction_ShouldHideDeveloperSurfaces(string path, HttpStatusCode expected)
        {
            using (HttpClient client = _factory.CreateClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(path))
                {
                    Assert.That(response.StatusCode, Is.EqualTo(expected));
                }
            }
        }
    }
}

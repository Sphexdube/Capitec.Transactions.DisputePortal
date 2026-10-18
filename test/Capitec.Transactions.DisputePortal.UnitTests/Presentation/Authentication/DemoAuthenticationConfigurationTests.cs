using Capitec.Transactions.DisputePortal.Api.Authentication;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Capitec.Transactions.DisputePortal.UnitTests.Presentation.Authentication
{
    public sealed class DemoAuthenticationConfigurationTests
    {
        [TestCase(true, "Development", true)]
        [TestCase(true, "Production", false)]
        [TestCase(false, "Local", false)]
        public void Decisions_WhenFlagAndEnvironmentVary_ShouldOnlyAllowLocalUse(bool flag, string environmentName, bool expected)
        {
            IHostEnvironment environment = Substitute.For<IHostEnvironment>();
            environment.EnvironmentName.Returns(environmentName);
            DemoAuthenticationConfiguration configuration = new() { Enabled = flag };

            Assert.That(configuration.IsAvailableIn(environment), Is.EqualTo(expected));
        }
    }
}

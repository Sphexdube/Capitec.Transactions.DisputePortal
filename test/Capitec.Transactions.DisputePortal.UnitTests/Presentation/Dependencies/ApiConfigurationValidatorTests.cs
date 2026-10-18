using Capitec.Transactions.DisputePortal.Api.Dependencies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Capitec.Transactions.DisputePortal.UnitTests.Presentation.Dependencies
{
    public sealed class ApiConfigurationValidatorTests
    {
        [Test]
        public void Validate_WhenSectionsAreSet_ShouldBindTypedConfiguration()
        {
            IConfiguration configuration = Build(new Dictionary<string, string?>
            {
                ["Authentication:SigningKey"] = new string('k', 32),
                ["Authentication:Issuer"] = "Issuer",
                ["DemoAuthentication:Enabled"] = "true",
                ["Persistence:MaxRetryCount"] = "7",
                ["ConfirmationWorker:BatchSize"] = "9"
            });

            ApiConfiguration result = ApiConfigurationValidator.Validate(configuration);

            Assert.That(result.Authentication.Issuer, Is.EqualTo("Issuer"));
            Assert.That(result.DemoAuthentication.Enabled, Is.True);
            Assert.That(result.Persistence.MaxRetryCount, Is.EqualTo(7));
            Assert.That(result.ConfirmationWorker.BatchSize, Is.EqualTo(9));
        }

        [Test]
        public void Validate_WhenOptionalSectionsAreMissing_ShouldUseDefaults()
        {
            ApiConfiguration result = ApiConfigurationValidator.Validate(Build(new Dictionary<string, string?>
            {
                ["Authentication:SigningKey"] = new string('k', 32)
            }));

            Assert.That(result.DemoAuthentication.Enabled, Is.False);
            Assert.That(result.Persistence.MaxRetryCount, Is.EqualTo(3));
            Assert.That(result.ConfirmationWorker.Enabled, Is.False);
        }

        [TestCase(null)]
        [TestCase("too-short")]
        public void Validate_WhenSigningKeyIsShorterThan32Bytes_ShouldThrowInvalidOperationException(string? signingKey)
        {
            IConfiguration configuration = Build(new Dictionary<string, string?> { ["Authentication:SigningKey"] = signingKey });

            Assert.Throws<InvalidOperationException>(() => ApiConfigurationValidator.Validate(configuration));
        }

        [Test]
        public void Validate_WhenConfigurationIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ApiConfigurationValidator.Validate(null!));
        }

        [TestCase("Development", true)]
        [TestCase("Local", true)]
        [TestCase("Production", false)]
        public void IsLocalOrDevelopment_WhenEnvironmentIsGiven_ShouldMatchLocalEnvironments(string name, bool expected)
        {
            IHostEnvironment environment = Substitute.For<IHostEnvironment>();
            environment.EnvironmentName.Returns(name);

            Assert.That(ApiEnvironments.IsLocalOrDevelopment(environment), Is.EqualTo(expected));
        }

        [Test]
        public void IsLocalOrDevelopment_WhenEnvironmentIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ApiEnvironments.IsLocalOrDevelopment(null!));
        }

        private static IConfiguration Build(Dictionary<string, string?> values)
        {
            return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        }
    }
}

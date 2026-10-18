using System.IdentityModel.Tokens.Jwt;
using Capitec.Transactions.DisputePortal.Api.Authentication;
using Capitec.Transactions.DisputePortal.Domain.Authentication;

namespace Capitec.Transactions.DisputePortal.UnitTests.Presentation.Authentication
{
    public sealed class JwtDemoTokenIssuerTests
    {
        [Test]
        public void Issue_WhenCustomerIsKnown_ShouldSignTokenWithCustomerClaim()
        {
            AuthenticationConfiguration configuration = new() { SigningKey = new string('k', 32) };
            JwtDemoTokenIssuer issuer = new(configuration, TimeProvider.System);

            DemoToken token = issuer.Issue(7);

            JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
            Assert.That(jwt.Claims.Single(claim => claim.Type == CustomerClaimTypes.CustomerId).Value, Is.EqualTo("7"));
            Assert.That(jwt.Subject, Is.EqualTo("demo-customer-7"));
            Assert.That(jwt.Issuer, Is.EqualTo(configuration.Issuer));
            Assert.That(jwt.Audiences.Single(), Is.EqualTo(configuration.Audience));
            Assert.That(token.ExpiresAt - DateTimeOffset.UtcNow, Is.LessThanOrEqualTo(configuration.TokenLifetime));
        }
    }
}

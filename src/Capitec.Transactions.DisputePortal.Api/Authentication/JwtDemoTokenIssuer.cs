using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Capitec.Transactions.DisputePortal.Domain.Authentication;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Capitec.Transactions.DisputePortal.Api.Authentication
{
    public sealed class JwtDemoTokenIssuer(
        AuthenticationConfiguration configuration,
        TimeProvider timeProvider) : IDemoTokenIssuer
    {
        public DemoToken Issue(long customerId)
        {
            DateTimeOffset issuedAt = timeProvider.GetUtcNow();
            DateTimeOffset expiresAt = issuedAt.Add(configuration.TokenLifetime);
            SigningCredentials credentials = new(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.SigningKey)),
                SecurityAlgorithms.HmacSha256);
            string customerIdValue = customerId.ToString(CultureInfo.InvariantCulture);
            Claim[] claims =
            [
                new Claim(CustomerClaimTypes.CustomerId, customerIdValue),
                new Claim(JwtRegisteredClaimNames.Sub, CustomerClaimTypes.DemoSubjectPrefix + customerIdValue)
            ];
            JwtSecurityToken token = new(
                configuration.Issuer,
                configuration.Audience,
                claims,
                issuedAt.UtcDateTime,
                expiresAt.UtcDateTime,
                credentials);

            return new DemoToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}

using System.Globalization;
using System.Security.Claims;
using Capitec.Transactions.DisputePortal.Api.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Capitec.Transactions.DisputePortal.UnitTests.Setup
{
    public static class ControllerContextFactory
    {
        public const string TraceIdentifier = "trace-unit";

        public static ControllerContext Create(long? customerId)
        {
            ClaimsIdentity identity = customerId.HasValue
                ? new ClaimsIdentity([new Claim(CustomerClaimTypes.CustomerId, customerId.Value.ToString(CultureInfo.InvariantCulture))], "Test")
                : new ClaimsIdentity();

            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity),
                    TraceIdentifier = TraceIdentifier
                }
            };
        }
    }
}

using System.Text;
using Capitec.Transactions.DisputePortal.Api.Authentication;
using Capitec.Transactions.DisputePortal.Api.Results;
using Capitec.Transactions.DisputePortal.Domain.Errors;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Capitec.Transactions.DisputePortal.Api.Dependencies
{
    public static class AuthenticationRegistration
    {
        public static void Register(IServiceCollection services, AuthenticationConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = LogAuthenticationFailure,
                        OnChallenge = WriteChallengeAsync
                    };
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.SigningKey)),
                        ValidateIssuer = true,
                        ValidIssuer = configuration.Issuer,
                        ValidateAudience = true,
                        ValidAudience = configuration.Audience,
                        ValidateLifetime = true,
                        ClockSkew = configuration.ClockSkew
                    };
                });

            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        }

        private static Task LogAuthenticationFailure(AuthenticationFailedContext context)
        {
            ILogger logger = context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(AuthenticationRegistration));
            logger.LogWarning(WarningMessages.BearerAuthenticationFailed, context.Exception.GetType().Name);

            return Task.CompletedTask;
        }

        private static async Task WriteChallengeAsync(JwtBearerChallengeContext context)
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(ApiResponseFactory.CreateError(
                context.HttpContext,
                ApiErrorCodes.Unauthenticated,
                ErrorMessages.CustomerSessionRequired));
        }
    }
}

namespace Capitec.Transactions.DisputePortal.Api.Authentication
{
    public sealed record AuthenticationConfiguration
    {
        public const string SectionName = "Authentication";
        public const int MinimumSigningKeyBytes = 32;

        public string Issuer { get; init; } = "DisputePortal";

        public string Audience { get; init; } = "DisputePortal.Web";

        public string SigningKey { get; init; } = string.Empty;

        public TimeSpan TokenLifetime { get; init; } = TimeSpan.FromMinutes(15);

        public TimeSpan ClockSkew { get; init; } = TimeSpan.FromSeconds(30);
    }
}

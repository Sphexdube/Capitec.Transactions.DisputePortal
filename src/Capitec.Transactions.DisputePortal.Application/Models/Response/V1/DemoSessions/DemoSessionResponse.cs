namespace Capitec.Transactions.DisputePortal.Application.Models.Response.V1.DemoSessions
{
    /// <summary>A signed demo session.</summary>
    public sealed record DemoSessionResponse
    {
        /// <summary>The bearer access token.</summary>
        public required string AccessToken { get; init; }

        /// <summary>The display name of the signed-in demo customer.</summary>
        public required string DisplayName { get; init; }

        /// <summary>The time the access token expires.</summary>
        public required DateTimeOffset ExpiresAt { get; init; }
    }
}

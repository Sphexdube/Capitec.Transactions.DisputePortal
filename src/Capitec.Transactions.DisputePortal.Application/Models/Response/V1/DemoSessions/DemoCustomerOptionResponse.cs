namespace Capitec.Transactions.DisputePortal.Application.Models.Response.V1.DemoSessions
{
    /// <summary>A demo customer a session can start for.</summary>
    public sealed record DemoCustomerOptionResponse
    {
        /// <summary>The id of the demo customer.</summary>
        public required long CustomerId { get; init; }

        /// <summary>The display name of the demo customer.</summary>
        public required string DisplayName { get; init; }
    }
}

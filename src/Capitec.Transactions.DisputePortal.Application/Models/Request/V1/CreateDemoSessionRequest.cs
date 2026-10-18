namespace Capitec.Transactions.DisputePortal.Application.Models.Request.V1
{
    /// <summary>Starts a demo session for one seeded demo customer.</summary>
    public sealed record CreateDemoSessionRequest
    {
        /// <summary>The id of the demo customer to sign in as.</summary>
        public required long CustomerId { get; init; }
    }
}

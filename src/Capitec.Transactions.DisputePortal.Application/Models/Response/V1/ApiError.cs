namespace Capitec.Transactions.DisputePortal.Application.Models.Response.V1
{
    /// <summary>One error in an error response.</summary>
    public sealed record ApiError
    {
        /// <summary>The stable machine-readable error code.</summary>
        public required string Code { get; init; }

        /// <summary>The message that is safe to show to the customer.</summary>
        public required string Message { get; init; }
    }
}

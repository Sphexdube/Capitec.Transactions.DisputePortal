namespace Capitec.Transactions.DisputePortal.Application.Models.Request.V1
{
    /// <summary>Gets the status history of one dispute owned by the signed-in customer.</summary>
    public sealed record GetDisputeHistoryRequest
    {
        /// <summary>The external id of the dispute.</summary>
        public required Guid DisputeId { get; init; }

        /// <summary>The id of the signed-in customer.</summary>
        public required long CustomerId { get; init; }
    }
}

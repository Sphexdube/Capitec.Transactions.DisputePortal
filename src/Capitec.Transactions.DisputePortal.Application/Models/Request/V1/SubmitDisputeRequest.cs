namespace Capitec.Transactions.DisputePortal.Application.Models.Request.V1
{
    /// <summary>Submits a dispute against a transaction owned by the signed-in customer.</summary>
    public sealed record SubmitDisputeRequest
    {
        /// <summary>The external id of the disputed transaction.</summary>
        public required Guid TransactionId { get; init; }

        /// <summary>The id of the signed-in customer.</summary>
        public required long CustomerId { get; init; }

        /// <summary>The client key that makes a retried submission safe.</summary>
        public required string IdempotencyKey { get; init; }

        /// <summary>The reason the customer disputes the transaction.</summary>
        public required string Reason { get; init; }

        /// <summary>The optional explanation from the customer.</summary>
        public string? Explanation { get; init; }
    }
}

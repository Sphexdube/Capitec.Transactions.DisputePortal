namespace Capitec.Transactions.DisputePortal.Application.Models.Request.V1
{
    /// <summary>Gets one transaction owned by the signed-in customer.</summary>
    public sealed record GetTransactionRequest
    {
        /// <summary>The external id of the transaction.</summary>
        public required Guid TransactionId { get; init; }

        /// <summary>The id of the signed-in customer.</summary>
        public required long CustomerId { get; init; }
    }
}
